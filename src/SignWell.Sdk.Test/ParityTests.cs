using System.Security.Cryptography;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SignWell.Sdk.Embedded;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;
using SignWell.Sdk.Resources;
using SignWell.Sdk.Client;
using SignWell.Sdk.Webhooks;
using Xunit;

namespace SignWell.Sdk.Test;

public sealed class ParityTests
{
    [Fact]
    public void ClientRequiresApiKeyAndSafeBaseUri()
    {
        Assert.Throws<ArgumentException>(() => new SignWellClient(" "));
        Assert.Throws<ArgumentException>(() => new SignWellClient("secret", new Uri("http://example.com")));
        Assert.Throws<ArgumentException>(() => new SignWellClient("secret", new Uri("https://user@example.com")));
    }

    [Fact]
    public void DefaultUserAgentTracksThePackageVersion()
    {
        var informational = typeof(SignWellClientOptions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Assert.NotNull(informational);
        Assert.Equal($"signwell-sdk-csharp/{informational!.Split('+')[0]}", SignWellClientOptions.UserAgent);
        Assert.DoesNotContain("+", SignWellClientOptions.UserAgent);
    }

    [Fact]
    public void DependencyInjectionExposesFacadeAndAllRawResources()
    {
        var services = new ServiceCollection();
        services.AddSignWell(o => o.ApiKey = "test-key");
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<SignWellClient>();
        Assert.False((bool)typeof(SignWellClient)
            .GetField("_ownsProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(client)!);
        Assert.NotNull(client.Documents.Raw);
        Assert.NotNull(provider.GetRequiredService<IDocumentApi>());
        Assert.NotNull(provider.GetRequiredService<ITemplateApi>());
    }

    [Theory]
    [InlineData(true, "t")]
    [InlineData(false, "f")]
    [InlineData("true", "t")]
    [InlineData("FALSE", "f")]
    [InlineData("t", "t")]
    [InlineData("f", "f")]
    public void CheckboxNormalizationAcceptsOnlyUnambiguousValues(object value, string expected) =>
        Assert.Equal(expected, SignWellValues.NormalizeCheckbox(value));

    [Theory]
    [InlineData("yes")]
    [InlineData("no")]
    [InlineData("1")]
    [InlineData("0")]
    public void CheckboxNormalizationRejectsAmbiguousValues(string value) =>
        Assert.Throws<ArgumentException>(() => SignWellValues.NormalizeCheckbox(value));

    [Fact]
    public void CheckboxFieldConvertersNormalizeTheActualRequestBody()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FieldTypeJsonConverter());
        options.Converters.Add(new AdditionalFieldsInnerInnerValueJsonConverter());
        options.Converters.Add(new FieldsInnerInnerJsonConverter());

        var checkbox = new FieldsInnerInner(10, 20, 1, "1", FieldType.Checkbox,
            value: new AdditionalFieldsInnerInnerValue(true));
        var text = new FieldsInnerInner(10, 20, 1, "1", FieldType.Text,
            value: new AdditionalFieldsInnerInnerValue("true"));

        Assert.Contains("\"value\":\"t\"", JsonSerializer.Serialize(checkbox, options));
        Assert.Contains("\"value\":\"true\"", JsonSerializer.Serialize(text, options));
        Assert.Throws<ArgumentException>(() => JsonSerializer.Serialize(
            new FieldsInnerInner(10, 20, 1, "1", FieldType.Checkbox,
                value: new AdditionalFieldsInnerInnerValue("yes")), options));
    }

    [Fact]
    public void RendererAppliesNonceAndEscapesScriptContext()
    {
        var rendered = EmbeddedRenderer.SigningIframe(new EmbeddedIframeOptions(
            "https://www.signwell.com/sign/abc?value=%3C/script%3E",
            Events: new Dictionary<string, string> { ["completed"] = "App.handlers.completed" },
            Nonce: "abc\" onload=\"bad"));
        Assert.Contains("nonce=\"abc&quot; onload=&quot;bad\"", rendered);
        Assert.DoesNotContain("</script><script>", rendered, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("App.handlers.completed", rendered);
    }

    [Theory]
    [InlineData("https://evil.example/sign")]
    [InlineData("http://www.signwell.com/sign")]
    [InlineData("https://user:pass@www.signwell.com/sign")]
    public void RendererRejectsUnsafeEmbedUrls(string url) =>
        Assert.Throws<ArgumentException>(() => EmbeddedRenderer.SigningIframe(new EmbeddedIframeOptions(url)));

    [Theory]
    [InlineData("App.constructor.run")]
    [InlineData("App.__proto__.run")]
    [InlineData("alert(1)")]
    public void RendererRejectsUnsafeHandlerPaths(string handler) =>
        Assert.Throws<ArgumentException>(() => EmbeddedRenderer.SigningIframe(new EmbeddedIframeOptions(
            "https://www.signwell.com/sign/abc",
            Events: new Dictionary<string, string> { ["completed"] = handler })));

    [Fact]
    public async Task WebhookVerificationIsFreshTimingSafeAndReplayProtected()
    {
        const string webhookId = "hook-secret";
        const long timestamp = 1_700_000_000;
        var hash = Hmac(webhookId, $"document_completed@{timestamp}");
        using var document = JsonDocument.Parse($$"""{"type":"document_completed","time":{{timestamp}},"hash":"{{hash}}"}""");
        var now = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        Assert.True(WebhookVerifier.Verify(document.RootElement, webhookId, TimeSpan.FromMinutes(5), now));
        Assert.False(WebhookVerifier.Verify(document.RootElement, "wrong", TimeSpan.FromMinutes(5), now));
        Assert.False(WebhookVerifier.Verify(document.RootElement, webhookId, TimeSpan.FromMinutes(5), now.AddMinutes(6)));

        var store = new InMemoryWebhookReplayStore(clock: () => now);
        Assert.True(await WebhookVerifier.VerifyOnceAsync(document.RootElement, webhookId, store, TimeSpan.FromMinutes(5), now));
        Assert.False(await WebhookVerifier.VerifyOnceAsync(document.RootElement, webhookId, store, TimeSpan.FromMinutes(5), now));
    }

    [Fact]
    public async Task ReplayStoreFailsClosedAtCapacity()
    {
        var store = new InMemoryWebhookReplayStore(1, () => DateTimeOffset.UnixEpoch);
        Assert.True(await store.TryAddAsync("first", DateTimeOffset.MaxValue));
        await Assert.ThrowsAsync<WebhookReplayCapacityException>(
            async () => await store.TryAddAsync("second", DateTimeOffset.MaxValue));
    }

    [Fact]
    public void NestedValidationErrorsDeserializeRecursively()
    {
        var value = JsonSerializer.Deserialize<ValidationErrorValue>(
            """{"recipients":{"email":["is invalid","is required"]}}""");
        Assert.Equal("is invalid", value!.Dictionary!["recipients"].Dictionary!["email"].List![0]);
    }

    [Fact]
    public void UnknownEnumsUseTheFallbackMember()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FileFormatJsonConverter());
        Assert.Equal(FileFormat.UnknownDefaultOpenApi, JsonSerializer.Deserialize<FileFormat>("\"future-format\"", options));
    }

    [Fact]
    public async Task BinaryDownloadsAreUnbufferedAndOwnTheResponse()
    {
        var content = new TrackingContent(new byte[2_000_000]);
        var handler = new CaptureHandler(content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.signwell.com") };
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", "conflicting-default");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", SignWellClientOptions.UserAgent);
        var transport = new ResponseModesClient(
            new SingleClientFactory(httpClient),
            new SignWellClientOptions { ApiKey = "real-key" },
            new JsonSerializerOptionsProvider(new JsonSerializerOptions()));

        var stream = await transport.GetCompletedPdfStreamAsync(Guid.Empty);
        Assert.False(content.WasSerialized);
        Assert.False(content.WasDisposed);
        Assert.Equal("real-key", handler.ApiKey);
        Assert.Equal(SignWellClientOptions.UserAgent, handler.UserAgent);
        stream.Dispose();
        Assert.True(content.WasDisposed);
        Assert.True(content.Stream.WasDisposed);
    }

    private static string Hmac(string key, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return string.Concat(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(b => b.ToString("x2")));
    }

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class CaptureHandler(TrackingContent content) : HttpMessageHandler
    {
        public string? ApiKey { get; private set; }
        public string? UserAgent { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ApiKey = string.Join(",", request.Headers.GetValues("X-Api-Key"));
            UserAgent = string.Join(" ", request.Headers.GetValues("User-Agent"));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class TrackingContent : HttpContent
    {
        public TrackingContent(byte[] bytes)
        {
            Stream = new TrackingStream(bytes);
            Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        }

        public TrackingStream Stream { get; }
        public bool WasSerialized { get; private set; }
        public bool WasDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            WasSerialized = true;
            return Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult<Stream>(Stream);

        protected override void Dispose(bool disposing)
        {
            if (disposing) WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackingStream(byte[] bytes) : MemoryStream(bytes)
    {
        public bool WasDisposed { get; private set; }
        protected override void Dispose(bool disposing)
        {
            if (disposing) WasDisposed = true;
            base.Dispose(disposing);
        }
    }
}
