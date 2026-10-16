using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SignWell.Sdk.Client;
using SignWell.Sdk.Embedded;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;
using SignWell.Sdk.Resources;
using SignWell.Sdk.Webhooks;
using Xunit;

namespace SignWell.Sdk.Test;

public sealed class SdkParityHarnessTests
{
    [Fact]
    public async Task GetMeDeserializesUuidMembershipIds()
    {
        // Membership ids are AccountRole external ids, always UUIDs (Trello 2708, AC-04), so
        // MeResponse.Id stays a Guid; only the user ids are plain strings (see the legacy test).
        const string id = "2efeb26c-f151-4ff7-9341-81d5741f5e28";
        const string relatedId = "b4905391-fd3b-402c-a8ca-b3201a5549c0";
        using var client = Client((request, _) =>
        {
            Assert.Equal("/api/v1/me", request.RequestUri!.AbsolutePath);
            return Task.FromResult(Json(HttpStatusCode.OK, $$"""
                {
                    "id":"{{id}}", "role":"owner",
                    "user":{"id":"{{relatedId}}","name":"Test","email":"test@example.com"},
                    "account":{"id":"{{relatedId}}","name":"Test","plan_tier":"free"},
                    "workspace":{"id":"{{relatedId}}","name":"Test","plan_tier":"free"},
                    "contact":{"id":"{{relatedId}}","name":"Test","email":"test@example.com"}
                }
                """));
        });

        var me = await client.GetMeAsync();

        Assert.Equal(Guid.Parse(id), me.Id);
    }

    [Fact]
    public void RawClientsExposeAllOpenApiOperations()
    {
        var actual = new[]
        {
            typeof(IApiApplicationApi), typeof(IBulkSendApi), typeof(IDocumentApi),
            typeof(IMeApi), typeof(IRegionalApi), typeof(ITemplateApi), typeof(IWebhooksApi)
        }
        .SelectMany(type => type.GetMethods())
        .Select(method => method.Name)
        .Where(name => name.EndsWith("Async", StringComparison.Ordinal) &&
                       !name.EndsWith("OrDefaultAsync", StringComparison.Ordinal))
        .ToHashSet(StringComparer.Ordinal);

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "CreateBulkSendAsync", "CreateDocumentAsync", "CreateDocumentFromTemplateAsync",
            "CreateTemplateAsync", "CreateWebhookAsync", "DeleteApiApplicationAsync",
            "DeleteDocumentAsync", "DeleteTemplateAsync", "DeleteWebhookAsync",
            "GetApiApplicationAsync", "GetBulkSendAsync", "GetBulkSendCsvTemplateAsync",
            "GetBulkSendDocumentsAsync", "GetCompletedPdfAsync", "GetDocumentAsync", "GetMeAsync",
            "GetNom151CertificateAsync", "GetTemplateAsync", "ListBulkSendsAsync",
            "ListDocumentsAsync", "ListTemplatesAsync", "ListWebhooksAsync", "SendDocumentAsync",
            "SendReminderAsync", "UpdateAuthenticationAsync", "UpdateRecipientsAsync", "UpdateTemplateAsync",
            "ValidateBulkSendCsvAsync"
        };

        Assert.Equal(28, actual.Count);
        Assert.True(expected.SetEquals(actual), $"Missing: {string.Join(", ", expected.Except(actual))}; extra: {string.Join(", ", actual.Except(expected))}");
    }

    [Fact]
    public void ClientSurfaceExposesEveryResourceAndRootConvenienceWorkflow()
    {
        using var client = new SignWellClient("test-key");
        Assert.All(new object[]
        {
            client.Documents, client.Templates, client.BulkSends, client.Regional,
            client.Me, client.ApiApplications, client.Webhooks, client.Embedded,
            client.Documents.Raw, client.Templates.Raw, client.BulkSends.Raw,
            client.Regional.Raw, client.Me.Raw, client.ApiApplications.Raw, client.Webhooks.Raw
        }, Assert.NotNull);

        var methods = typeof(ISignWellClient).GetMethods().Select(method => method.Name).ToHashSet();
        var expected = new HashSet<string>
        {
            "CreateDocumentAsync", "CreateDocumentFromTemplateAsync", "SendDocumentAsync",
            "UpdateDocumentAsync", "SendReminderAsync", "GetMeAsync", "CreateTemplateAsync",
            "UpdateTemplateAsync", "CreateBulkSendAsync", "ValidateBulkSendCsvAsync",
            "GetCompletedPdfStreamAsync", "GetCompletedPdfUrlAsync"
        };
        Assert.All(expected, method => Assert.Contains(method, methods));
    }

    [Fact]
    public void OptionsNormalizeUrlsAndRejectUnsafeValues()
    {
        var options = new SignWellClientOptions
        {
            ApiKey = "key",
            BaseUri = new Uri("https://api.example.com/root///")
        };
        options.Validate();
        Assert.Equal("https://api.example.com/root", options.BaseUri.AbsoluteUri.TrimEnd('/'));
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);

        Assert.Throws<ArgumentException>(() => SignWellClientOptions.NormalizeBaseUri(new Uri("https://example.com?q=secret")));
        Assert.Throws<ArgumentException>(() => SignWellClientOptions.NormalizeBaseUri(new Uri("https://example.com/#fragment")));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SignWellClient(new SignWellClientOptions
        {
            ApiKey = "key",
            Timeout = TimeSpan.Zero
        }));
    }

    [Fact]
    public async Task PaginationUsesRawFiltersClampsLimitsAndIteratesDocumentsAndTemplates()
    {
        var documentCalls = 0;
        var templateCalls = 0;
        var requests = new ConcurrentBag<Uri>();
        var firstDocument = Guid.NewGuid();
        var secondDocument = Guid.NewGuid();
        var firstTemplate = Guid.NewGuid();
        var secondTemplate = Guid.NewGuid();
        using var client = Client((request, _) =>
        {
            requests.Add(request.RequestUri!);
            if (request.RequestUri!.AbsolutePath == "/api/v1/documents")
            {
                var call = Interlocked.Increment(ref documentCalls);
                return Task.FromResult(Json(HttpStatusCode.OK,
                    $$"""{"documents":[{"test_mode":true,"id":"{{(call == 1 ? firstDocument : secondDocument)}}"}],"current_page":{{call}},"total_count":2,"total_pages":2,"next_page":{{(call == 1 ? "2" : "null")}}}"""));
            }
            if (request.RequestUri.AbsolutePath == "/api/v1/document_templates")
            {
                var call = Interlocked.Increment(ref templateCalls);
                return Task.FromResult(Json(HttpStatusCode.OK,
                    $$"""{"templates":[{"id":"{{(call == 1 ? firstTemplate : secondTemplate)}}"}],"current_page":{{call}},"total_count":2,"total_pages":2,"next_page":{{(call == 1 ? "2" : "null")}}}"""));
            }
            throw new InvalidOperationException(request.RequestUri.AbsoluteUri);
        });

        var documents = new List<Guid>();
        await foreach (var document in client.Documents.GetItemsAsync("status:Completed", limit: 500))
            documents.Add(document.Id);
        var templates = new List<Guid>();
        await foreach (var template in client.Templates.GetItemsAsync("name:Reusable", limit: 500))
            templates.Add(template.Id);

        Assert.Equal(new[] { firstDocument, secondDocument }, documents);
        Assert.Equal(new[] { firstTemplate, secondTemplate }, templates);
        Assert.All(requests, uri => Assert.Contains("limit=50", uri.Query));
        Assert.Contains(requests, uri => Uri.UnescapeDataString(uri.Query).Contains("query=status:Completed", StringComparison.Ordinal));
        Assert.Contains(requests, uri => Uri.UnescapeDataString(uri.Query).Contains("query=name:Reusable", StringComparison.Ordinal));

        var limit = 100;
        DocumentsResource.ValidateListArguments("query", 1, ref limit);
        Assert.Equal(50, limit);
        Assert.Throws<ArgumentException>(() => DocumentsResource.ValidateListArguments(" ", 1, ref limit));
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentsResource.ValidateListArguments(null, 0, ref limit));
    }

    [Fact]
    public async Task PollingSupportsSuccessOverridesTimeoutLastResponseCancellationAndUpdateAlias()
    {
        var id = Guid.NewGuid();
        var getCalls = 0;
        var sendCalls = 0;
        using var client = Client((request, _) =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.TrimEnd('/').EndsWith(id.ToString("D"), StringComparison.Ordinal))
            {
                var status = Interlocked.Increment(ref getCalls) >= 2 ? "Completed" : "Sent";
                return Task.FromResult(Json(HttpStatusCode.OK, $$"""{"test_mode":true,"id":"{{id}}","status":"{{status}}"}"""));
            }
            if (request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath.TrimEnd('/').EndsWith("/send", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref sendCalls);
                return Task.FromResult(Json(HttpStatusCode.Created, $$"""{"test_mode":true,"id":"{{id}}","status":"Sent"}"""));
            }
            throw new InvalidOperationException(request.RequestUri?.AbsoluteUri);
        });

        var completed = await client.Documents.WaitForCompletionAsync(id, TimeSpan.FromMilliseconds(1), maximumAttempts: 3);
        Assert.Equal("Completed", completed.Status);
        getCalls = 0;
        var sent = await client.Documents.WaitForCompletionAsync(id, terminalStatuses: new[] { "Sent" }, maximumAttempts: 1);
        Assert.Equal("Sent", sent.Status);
        var updated = await client.UpdateDocumentAsync(id, new UpdateDocumentAndSendRequest());
        Assert.Equal(id, updated.Id);
        Assert.Equal(1, sendCalls);

        getCalls = 0;
        var timeout = await Assert.ThrowsAsync<WaitForCompletionTimeoutException>(() =>
            client.Documents.WaitForCompletionAsync(id, TimeSpan.FromMilliseconds(1), maximumAttempts: 1, terminalStatuses: new[] { "Expired" }));
        Assert.Equal("Sent", timeout.LastResponse?.Status);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Documents.WaitForCompletionAsync(id, cancellationToken: cancellation.Token));
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Manually completed")]
    [InlineData("Declined")]
    [InlineData("Canceled")]
    [InlineData("Bounced")]
    [InlineData("Blocked")]
    [InlineData("Error")]
    [InlineData("Expired")]
    public async Task PollingRecognizesAllEightDefaultTerminalStatuses(string status)
    {
        var id = Guid.NewGuid();
        using var client = Client((_, _) => Task.FromResult(Json(
            HttpStatusCode.OK,
            $$"""{"test_mode":true,"id":"{{id}}","status":"{{status}}"}""")));

        var response = await client.Documents.WaitForCompletionAsync(id, maximumAttempts: 1);

        Assert.Equal(status, response.Status);
    }

    [Fact]
    public async Task RawClientsPreserveResponseHeaders()
    {
        var id = Guid.NewGuid();
        using var client = Client((_, _) =>
        {
            var response = Json(HttpStatusCode.OK, $$"""{"test_mode":true,"id":"{{id}}"}""");
            response.Headers.Add("X-SignWell-Test", "raw-header");
            return Task.FromResult(response);
        });

        var response = await client.Documents.Raw.GetDocumentAsync(id);
        Assert.Equal("raw-header", response.Headers.GetValues("X-SignWell-Test").Single());
        Assert.Equal(id, response.Ok()!.Id);
    }

    [Fact]
    public async Task EveryExplicitResponseModeUsesTheExpectedWireFlagsAndTypes()
    {
        var paths = new ConcurrentBag<string>();
        var handler = new DelegateHandler((request, _) =>
        {
            var path = request.RequestUri!.PathAndQuery;
            paths.Add(path);
            if (path.Contains("completed_pdf") && path.Contains("url_only=false"))
                return Task.FromResult(Binary("application/pdf", "%PDF-test\n%%EOF"));
            if (path.Contains("completed_pdf") && path.Contains("url_only=true"))
                return Task.FromResult(Json(HttpStatusCode.OK, """{"file_url":"https://files.signwell.com/document.pdf"}"""));
            if (path.Contains("csv_template") && path.Contains("base64=false"))
                return Task.FromResult(Binary("text/csv", "name,email\n"));
            if (path.Contains("csv_template") && path.Contains("base64=true"))
                return Task.FromResult(Json(HttpStatusCode.OK, """{"data":"bmFtZSxlbWFpbAo="}"""));
            if (path.Contains("nom151_certificate") && path.Contains("url_only=false&object_only=false"))
                return Task.FromResult(Binary("application/zip", "zip"));
            if (path.Contains("nom151_certificate") && path.Contains("url_only=true"))
                return Task.FromResult(Json(HttpStatusCode.OK, """{"file_url":"https://files.signwell.com/certificate.zip"}"""));
            if (path.Contains("nom151_certificate") && path.Contains("object_only=true"))
                return Task.FromResult(Json(HttpStatusCode.OK,
                    """{"nom151":{"status":"issued","transactionId":"tx","hash":"hash","folio":"folio","issuedAt":"2026-01-02T03:04:05Z","provider":"provider","validationUrl":"https://example.com/validate","constancia":"data"}}"""));
            throw new InvalidOperationException(path);
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.signwell.com") };
        var modes = Modes(httpClient);
        var id = Guid.NewGuid();
        var templates = new[] { Guid.NewGuid(), Guid.NewGuid() };

        using (var pdf = await modes.GetCompletedPdfStreamAsync(id, true, FileFormat.Pdf))
            Assert.Equal((byte)'%', pdf.ReadByte());
        Assert.EndsWith("document.pdf", (await modes.GetCompletedPdfUrlAsync(id)).FileUrl);
        using (var csv = await modes.GetBulkSendCsvStreamAsync(templates))
            Assert.Equal((byte)'n', csv.ReadByte());
        Assert.NotEmpty((await modes.GetBulkSendCsvBase64Async(templates)).Data);
        using (var zip = await modes.GetNom151StreamAsync(id))
            Assert.Equal((byte)'z', zip.ReadByte());
        Assert.EndsWith("certificate.zip", (await modes.GetNom151UrlAsync(id)).FileUrl);
        Assert.Equal("issued", (await modes.GetNom151ObjectAsync(id)).Nom151.Status);

        Assert.Contains(paths, path => path.Contains("url_only=false&audit_page=true&file_format=pdf", StringComparison.Ordinal));
        Assert.Contains(paths, path => path.Contains("url_only=true&audit_page=true", StringComparison.Ordinal));
        Assert.Contains(paths, path => path.Contains("template_ids%5B%5D=", StringComparison.Ordinal) && path.Contains("base64=true", StringComparison.Ordinal));
        Assert.All(paths.Where(path => path.Contains("csv_template", StringComparison.Ordinal)),
            path => Assert.Equal(2, path.Split("template_ids%5B%5D=", StringSplitOptions.None).Length - 1));
        Assert.Contains(paths, path => path.Contains("object_only=true", StringComparison.Ordinal));
        Assert.All(paths.Where(path => path.Contains("completed_pdf", StringComparison.Ordinal)),
            path => Assert.Contains("completed_pdf/?", path, StringComparison.Ordinal));
        Assert.All(paths.Where(path => path.Contains("csv_template", StringComparison.Ordinal)),
            path => Assert.Contains("csv_template/?", path, StringComparison.Ordinal));
        Assert.All(paths.Where(path => path.Contains("nom151_certificate", StringComparison.Ordinal)),
            path => Assert.Contains("nom151_certificate/?", path, StringComparison.Ordinal));
        await Assert.ThrowsAsync<ArgumentException>(() => modes.GetBulkSendCsvBase64Async(Array.Empty<Guid>()));
    }

    [Fact]
    public async Task ResponseModesHonorBaseUriPathPrefix()
    {
        Uri? seen = null;
        using var httpClient = new HttpClient(new DelegateHandler((request, _) =>
        {
            seen = request.RequestUri;
            return Task.FromResult(Json(HttpStatusCode.OK, """{"file_url":"https://files.signwell.com/document.pdf"}"""));
        }))
        { BaseAddress = new Uri("https://api.example.com/root") };
        var options = new SignWellClientOptions
        {
            ApiKey = "test-key",
            BaseUri = new Uri("https://api.example.com/root")
        };
        options.Validate();
        var modes = new ResponseModesClient(
            new SingleClientFactory(httpClient),
            options,
            new JsonSerializerOptionsProvider(new JsonSerializerOptions()));

        await modes.GetCompletedPdfUrlAsync(Guid.NewGuid());

        Assert.StartsWith("/root/api/v1/documents/", seen!.AbsolutePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAuthenticationPatchesPasscodeDelivery()
    {
        var id = Guid.NewGuid();
        Uri? seen = null;
        HttpMethod? method = null;
        string? body = null;
        using var client = Client(async (request, cancellationToken) =>
        {
            seen = request.RequestUri;
            method = request.Method;
            body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return Json(HttpStatusCode.OK, $$"""{"test_mode":true,"id":"{{id}}","status":"Sent"}""");
        });

        var updated = await client.Documents.UpdateAuthenticationAsync(
            id,
            new UpdateAuthenticationRequest(new List<UpdateAuthenticationRequestRecipientsInner>
            {
                new(id.ToString("D"), passcode: new Option<string?>("secret"))
            }));

        Assert.Equal(id, updated.Id);
        Assert.Equal(HttpMethod.Patch, method);
        Assert.Contains("/authentication", seen!.AbsolutePath, StringComparison.Ordinal);
        Assert.Contains("\"passcode\":\"secret\"", body);
    }

    [Fact]
    public async Task TransportContentTypeStatusAndRateLimitFailuresAreTypedAndRedacted()
    {
        var connectionCause = new HttpRequestException("network detail");
        var connectionCalls = 0;
        using (var client = new HttpClient(new DelegateHandler((_, _) =>
               {
                   Interlocked.Increment(ref connectionCalls);
                   return Task.FromException<HttpResponseMessage>(connectionCause);
               }))
        { BaseAddress = new Uri("https://www.signwell.com") })
        {
            var exception = await Assert.ThrowsAsync<ApiConnectionException>(() => Modes(client).GetCompletedPdfUrlAsync(Guid.NewGuid()));
            Assert.Same(connectionCause, exception.InnerException);
            Assert.Equal(1, connectionCalls);
        }

        var timeoutCause = new TaskCanceledException("client timeout");
        using (var client = new HttpClient(new DelegateHandler((_, _) => Task.FromException<HttpResponseMessage>(timeoutCause)))
        { BaseAddress = new Uri("https://www.signwell.com") })
        {
            var exception = await Assert.ThrowsAsync<ApiTimeoutException>(() => Modes(client).GetCompletedPdfUrlAsync(Guid.NewGuid()));
            Assert.Same(timeoutCause, exception.InnerException);
        }

        using (var client = new HttpClient(new DelegateHandler((_, _) => Task.FromResult(Binary("text/plain", "not-json"))))
        { BaseAddress = new Uri("https://www.signwell.com") })
        {
            await Assert.ThrowsAsync<UnsupportedContentTypeException>(() => Modes(client).GetCompletedPdfUrlAsync(Guid.NewGuid()));
        }

        var reset = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds();
        var response = Json((HttpStatusCode)429, """{"error":"private-response-body"}""");
        response.Headers.Add("X-RateLimit-Limit", "100");
        response.Headers.Add("X-RateLimit-Remaining", "0");
        response.Headers.Add("X-RateLimit-Reset", reset.ToString());
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(9));
        var rateLimit = await Assert.ThrowsAsync<RateLimitException>(() => ErrorMapper.ThrowIfErrorAsync(response, CancellationToken.None));
        Assert.Equal(100, rateLimit.RateLimit.Limit);
        Assert.Equal(0, rateLimit.RateLimit.Remaining);
        Assert.Equal(reset, rateLimit.RateLimit.Reset);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(reset), rateLimit.RateLimit.ResetTime);
        Assert.Equal(TimeSpan.FromSeconds(9), rateLimit.RateLimit.RetryAfter);
        Assert.NotNull(rateLimit.RateLimit.RetryTime);
        Assert.Equal("100", rateLimit.Headers["X-RateLimit-Limit"].Single());
        Assert.IsType<JsonElement>(rateLimit.Response);
        Assert.Contains("private-response-body", rateLimit.ResponseBody);
        Assert.DoesNotContain("private-response-body", rateLimit.Message);
        Assert.DoesNotContain("?", rateLimit.Message);
    }

    [Fact]
    public async Task EveryDocumentedHttpStatusMapsToItsCanonicalException()
    {
        var cases = new (HttpStatusCode Status, Type Type)[]
        {
            (HttpStatusCode.BadRequest, typeof(BadRequestException)),
            (HttpStatusCode.Unauthorized, typeof(AuthenticationException)),
            (HttpStatusCode.Forbidden, typeof(PermissionDeniedException)),
            (HttpStatusCode.NotFound, typeof(NotFoundException)),
            (HttpStatusCode.Conflict, typeof(ConflictException)),
            ((HttpStatusCode)422, typeof(UnprocessableEntityException)),
            ((HttpStatusCode)429, typeof(RateLimitException)),
            (HttpStatusCode.InternalServerError, typeof(InternalServerException))
        };

        foreach (var item in cases)
        {
            var response = Json(item.Status, "{}");
            var exception = await Record.ExceptionAsync(() => ErrorMapper.ThrowIfErrorAsync(response, CancellationToken.None));
            Assert.IsType(item.Type, exception);
            Assert.Equal(item.Status, Assert.IsAssignableFrom<ApiStatusException>(exception).StatusCode);
        }

        var permission = Assert.IsType<PermissionDeniedException>(await Record.ExceptionAsync(() =>
            ErrorMapper.ThrowIfErrorAsync(Json(HttpStatusCode.Forbidden, "{}"), CancellationToken.None)));
        Assert.IsAssignableFrom<ForbiddenException>(permission);
    }

    [Fact]
    public async Task UserCancellationRemainsOperationCanceledException()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var httpClient = new HttpClient(new DelegateHandler((_, token) => Task.FromCanceled<HttpResponseMessage>(token)))
        {
            BaseAddress = new Uri("https://www.signwell.com")
        };
        var exception = await Record.ExceptionAsync(() => Modes(httpClient).GetCompletedPdfUrlAsync(Guid.NewGuid(), cancellationToken: cancellation.Token));
        Assert.IsAssignableFrom<OperationCanceledException>(exception);
        Assert.IsNotType<ApiTimeoutException>(exception);
    }

    [Fact]
    public void ModelsUseSnakeCaseConditionalSerializationGuidAndDateTimeOffset()
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.Parse("2026-01-02T03:04:05+09:00");
        var model = new DocumentResponse(true, id, createdAt: created);
        var services = new ServiceCollection();
        _ = new HostConfiguration(services);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<JsonSerializerOptionsProvider>().Options;
        var json = JsonSerializer.Serialize(model, options);

        Assert.Contains($"\"id\":\"{id:D}\"", json);
        Assert.Contains("\"test_mode\":true", json);
        Assert.Contains("\"created_at\":", json);
        Assert.DoesNotContain("\"name\":", json);
        Assert.Equal(created, JsonSerializer.Deserialize<DocumentResponse>(json, options)!.CreatedAt);
    }

    [Fact]
    public void OptionalResponseFieldsAcceptExplicitNullsFromTheLiveApi()
    {
        var services = new ServiceCollection();
        _ = new HostConfiguration(services);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<JsonSerializerOptionsProvider>().Options;
        var id = Guid.NewGuid();

        var document = JsonSerializer.Deserialize<DocumentResponse>(
            $$"""{"test_mode":true,"id":"{{id}}","subject":null,"message":null,"recipients":null,"fields":[[{"x":1,"y":2,"page":1,"api_id":"Signature_1","value":"approved"}]]}""",
            options);

        Assert.NotNull(document);
        Assert.True(document.SubjectOption.IsSet);
        Assert.Null(document.Subject);
        Assert.True(document.RecipientsOption.IsSet);
        Assert.Null(document.Recipients);
        Assert.Equal("Signature_1", document.Fields![0][0].ApiId);
        Assert.Equal("approved", document.Fields[0][0].Value!.String);
        Assert.Equal("\"approved\"", JsonSerializer.Serialize(document.Fields[0][0].Value, options));

        var boolean = JsonSerializer.Deserialize<DocumentResponseFieldsInnerInnerValue>("true", options)!;
        Assert.True(boolean.Bool);
        Assert.Equal("true", JsonSerializer.Serialize(boolean, options));
        var uncheckedValue = JsonSerializer.Deserialize<DocumentResponseFieldsInnerInnerValue>("false", options)!;
        Assert.False(uncheckedValue.Bool);
        Assert.Equal("false", JsonSerializer.Serialize(uncheckedValue, options));
        var number = JsonSerializer.Deserialize<DocumentResponseFieldsInnerInnerValue>("12.5", options)!;
        Assert.Equal(12.5m, number.Decimal);
        Assert.Equal("12.5", JsonSerializer.Serialize(number, options));
    }

    [Fact]
    public void EmbeddedInputsValidateFilesTemplatesRecipientsPasscodesAndPlacement()
    {
        var pdf = Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF\n"));
        var files = EmbeddedModelFactory.Files(new[] { new EmbeddedFile("test.pdf", FileBase64: $" \n{pdf}\t") });
        Assert.NotEmpty(files[0].FileBase64!);
        Assert.Throws<ArgumentException>(() => EmbeddedModelFactory.Files(new[] { new EmbeddedFile("test.pdf") }));
        Assert.Throws<ArgumentException>(() => EmbeddedModelFactory.Files(new[] { new EmbeddedFile("test.pdf", "https://example.com/a.pdf", pdf) }));
        Assert.Throws<ArgumentException>(() => EmbeddedModelFactory.Files(new[] { new EmbeddedFile("test.pdf", FileBase64: "not-base64") }));
        Assert.Throws<ArgumentException>(() => EmbeddedModelFactory.Files(new[] { new EmbeddedFile("test.pdf", FileBase64: Convert.ToBase64String("%PDF-incomplete"u8.ToArray())) }));

        Assert.Throws<ArgumentException>(() => new EmbeddedTemplateSelector().Validate());
        Assert.Throws<ArgumentException>(() => new EmbeddedTemplateSelector(Guid.NewGuid(), new[] { Guid.NewGuid() }).Validate());
        new EmbeddedTemplateSelector(Guid.NewGuid()).Validate();
        new EmbeddedTemplateSelector(TemplateIds: new[] { Guid.NewGuid() }).Validate();

        var recipients = EmbeddedModelFactory.Recipients(new[]
        {
            new EmbeddedRecipient("One", "one@example.com", Passcode: 1234),
            new EmbeddedRecipient("Two", "two@example.com", Id: "custom", Passcode: "  secret  ")
        });
        Assert.Equal("1", recipients[0].Id);
        Assert.Equal("1234", recipients[0].Passcode);
        Assert.Equal("custom", recipients[1].Id);
        Assert.Equal("secret", recipients[1].Passcode);
        Assert.Throws<ArgumentException>(() => EmbeddedModelFactory.ValidatePlacement(recipients, null, false, false));
        EmbeddedModelFactory.ValidatePlacement(recipients, null, true, false);
    }

    [Fact]
    public async Task EmbeddedWorkflowsSerializeSigningRequestingSingleAndMultiTemplateModes()
    {
        var bodies = new Queue<string>();
        var documentId = Guid.NewGuid();
        using var client = Client(async (request, cancellationToken) =>
        {
            bodies.Enqueue(await request.Content!.ReadAsStringAsync(cancellationToken));
            return request.RequestUri!.AbsolutePath.TrimEnd('/') == "/api/v1/document_templates/documents"
                ? Json(HttpStatusCode.Created,
                    $$"""{"test_mode":true,"id":"{{documentId}}","recipients":[{"email":"signer@example.com","embedded_signing_url":"https://www.signwell.com/sign/template"}]}""")
                : Json(HttpStatusCode.Created,
                    $$"""{"test_mode":true,"id":"{{documentId}}","recipients":[{"name":"Signer","email":"signer@example.com","embedded_signing_url":"https://www.signwell.com/sign/document"}]}""");
        });
        var file = new EmbeddedFile("contract.pdf", FileUrl: "https://files.example.com/contract.pdf");
        var recipient = new EmbeddedRecipient("Signer", "signer@example.com", Passcode: 1234);
        IReadOnlyCollection<IReadOnlyCollection<EmbeddedField>> fields = new[]
        {
            (IReadOnlyCollection<EmbeddedField>)new[] { new EmbeddedField(10, 20, 1, FieldType.Signature, "1") }
        };

        var signing = await client.Embedded.CreateSigningDocumentAsync(
            "Signing", new[] { file }, new[] { recipient }, fields, sendNotifications: true);
        await client.Embedded.CreateRequestingDocumentAsync("Requesting", new[] { file }, new[] { recipient });
        var singleTemplate = await client.Embedded.CreateSigningDocumentFromTemplateAsync(
            new EmbeddedTemplateSelector(Guid.NewGuid()),
            new[] { new EmbeddedTemplateRecipient("Signer", "signer@example.com") });
        var templateIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await client.Embedded.CreateSigningDocumentFromTemplateAsync(
            new EmbeddedTemplateSelector(TemplateIds: templateIds),
            new[] { new EmbeddedTemplateRecipient("Signer", "signer@example.com") });

        using var signingJson = JsonDocument.Parse(bodies.Dequeue());
        Assert.True(signingJson.RootElement.GetProperty("embedded_signing").GetBoolean());
        Assert.True(signingJson.RootElement.GetProperty("embedded_signing_notifications").GetBoolean());
        Assert.False(signingJson.RootElement.TryGetProperty("draft", out _));
        Assert.Equal("1234", signingJson.RootElement.GetProperty("recipients")[0].GetProperty("passcode").GetString());
        Assert.Equal("signature", signingJson.RootElement.GetProperty("fields")[0][0].GetProperty("type").GetString());

        using var requestingJson = JsonDocument.Parse(bodies.Dequeue());
        Assert.True(requestingJson.RootElement.GetProperty("draft").GetBoolean());
        Assert.False(requestingJson.RootElement.TryGetProperty("embedded_signing", out _));

        using var singleJson = JsonDocument.Parse(bodies.Dequeue());
        Assert.True(singleJson.RootElement.TryGetProperty("template_id", out _));
        Assert.False(singleJson.RootElement.TryGetProperty("template_ids", out _));
        Assert.True(singleJson.RootElement.GetProperty("embedded_signing").GetBoolean());

        using var manyJson = JsonDocument.Parse(bodies.Dequeue());
        Assert.False(manyJson.RootElement.TryGetProperty("template_id", out _));
        Assert.Equal(2, manyJson.RootElement.GetProperty("template_ids").GetArrayLength());
        Assert.Empty(bodies);

        Assert.Equal("https://www.signwell.com/sign/document", EmbeddedWorkflows.SigningUrl(signing));
        Assert.Equal("https://www.signwell.com/sign/document", EmbeddedWorkflows.SigningUrls(signing)["SIGNER@example.com"]);
        Assert.Equal("https://www.signwell.com/sign/template", EmbeddedWorkflows.SigningUrl(singleTemplate));
        Assert.Equal("https://www.signwell.com/sign/template", EmbeddedWorkflows.SigningUrls(singleTemplate)["SIGNER@example.com"]);
    }

    [Fact]
    public void EmbeddedRendererRequiresExactAllowlistsForCustomEmbedAndRedirectHosts()
    {
        Assert.Contains("nonce=\"loader-nonce\"", EmbeddedRenderer.ScriptTag("loader-nonce"));
        var html = EmbeddedRenderer.RequestingIframe(new EmbeddedIframeOptions(
            "https://embed.example.com/request/1",
            AllowedEmbedHosts: new[] { "embed.example.com" },
            AllowedRedirectHosts: new[] { "app.example.com" },
            RedirectUrl: "https://app.example.com/done",
            ShowSendButton: true));
        Assert.Contains("showSendButton", html);
        Assert.Throws<ArgumentException>(() => EmbeddedRenderer.SigningIframe(new EmbeddedIframeOptions(
            "https://sub.embed.example.com/sign/1", AllowedEmbedHosts: new[] { "embed.example.com" })));
        Assert.Throws<ArgumentException>(() => EmbeddedRenderer.SigningIframe(new EmbeddedIframeOptions(
            "https://www.signwell.com/sign/1", RedirectUrl: "https://evil.example/done")));
    }

    [Fact]
    public async Task WebhookInvalidSignaturesAreNeverStoredAndConcurrentReplaysAcceptOnlyOnce()
    {
        const string webhookId = "secret";
        const long timestamp = 1_700_000_000;
        using var invalid = JsonDocument.Parse($$"""{"type":"document_completed","time":{{timestamp}},"hash":"bad"}""");
        var tracking = new TrackingReplayStore();
        Assert.False(await WebhookVerifier.VerifyOnceAsync(
            invalid.RootElement, webhookId, tracking, TimeSpan.FromMinutes(5), DateTimeOffset.FromUnixTimeSeconds(timestamp)));
        Assert.Equal(0, tracking.Calls);

        var hash = Hmac(webhookId, $"document_completed@{timestamp}");
        using var valid = JsonDocument.Parse($$"""{"type":"document_completed","time":{{timestamp}},"hash":"{{hash}}"}""");
        var store = new InMemoryWebhookReplayStore(clock: () => DateTimeOffset.FromUnixTimeSeconds(timestamp));
        var results = await Task.WhenAll(Enumerable.Range(0, 32).Select(async _ =>
            await WebhookVerifier.VerifyOnceAsync(valid.RootElement, webhookId, store, TimeSpan.FromMinutes(5), DateTimeOffset.FromUnixTimeSeconds(timestamp))));
        Assert.Equal(1, results.Count(result => result));

        var now = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var expiring = new InMemoryWebhookReplayStore(clock: () => now);
        Assert.True(await expiring.TryAddAsync("event", now.AddSeconds(1)));
        now = now.AddSeconds(2);
        Assert.True(await expiring.TryAddAsync("event", now.AddSeconds(1)));
    }

    [Fact]
    public async Task WebhookThrowingApisValidateShapeFreshnessReplayAndCapacity()
    {
        const string webhookId = "secret";
        const long timestamp = 1_700_000_000;
        var now = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var hash = Hmac(webhookId, $"document_completed@{timestamp}");
        using var valid = JsonDocument.Parse($$"""{"type":"document_completed","time":{{timestamp}},"hash":"{{hash}}"}""");
        using var malformed = JsonDocument.Parse("{}");
        using var extremeTimestamp = JsonDocument.Parse("""{"type":"document_completed","time":-9223372036854775808,"hash":"bad"}""");

        WebhookVerifier.VerifyOrThrow(valid.RootElement, webhookId, TimeSpan.FromMinutes(5), now.AddMinutes(5));
        Assert.False(WebhookVerifier.Verify(valid.RootElement, webhookId, TimeSpan.FromMinutes(5), now.AddMinutes(5).AddSeconds(1)));
        Assert.False(WebhookVerifier.Verify(extremeTimestamp.RootElement, webhookId, TimeSpan.FromMinutes(5), DateTimeOffset.UnixEpoch));
        Assert.Throws<WebhookVerificationException>(() => WebhookVerifier.VerifyOrThrow(malformed.RootElement, webhookId));
        Assert.Throws<WebhookVerificationException>(() => WebhookVerifier.VerifyOrThrow(valid.RootElement, webhookId, TimeSpan.FromSeconds(-1), now));

        var replay = new InMemoryWebhookReplayStore(clock: () => now);
        await WebhookVerifier.VerifyOnceOrThrowAsync(valid.RootElement, webhookId, replay, TimeSpan.FromMinutes(5), now);
        await Assert.ThrowsAsync<WebhookVerificationException>(async () =>
            await WebhookVerifier.VerifyOnceOrThrowAsync(valid.RootElement, webhookId, replay, TimeSpan.FromMinutes(5), now));

        var full = new InMemoryWebhookReplayStore(1, () => now);
        Assert.True(await full.TryAddAsync("occupied", now.AddMinutes(5)));
        await Assert.ThrowsAsync<WebhookReplayCapacityException>(async () =>
            await WebhookVerifier.VerifyOnceAsync(valid.RootElement, webhookId, full, TimeSpan.FromMinutes(5), now));
    }

    [Fact]
    public async Task GetMeDeserializesLegacyNumericUserIds()
    {
        // Users present at docsketch's Feb 2020 external_id backfill kept their AccountRole numeric id, so
        // the API returns user ids such as "3" for them. The spec used to declare user.id and
        // active_users[].id as format: uuid, which made this Guid-typed property throw on every legacy
        // account (Bidsketch/docsketch#4774). Membership, account and contact ids are real UUIDs.
        using var client = Client((_, _) => Task.FromResult(Json(HttpStatusCode.OK, LegacyMeResponse)));

        var me = await client.GetMeAsync();

        Assert.Equal("3", me.User.Id);
        Assert.Equal("3", Assert.Single(me.Account.ActiveUsers!).Id);
        Assert.Equal("3", Assert.Single(me.Workspace.ActiveUsers!).Id);
        // Membership, account and contact ids keep format: uuid; these pin that only the user ids
        // lost it, so dropping the format from the wrong line in the same schema block fails here.
        Assert.Equal(Guid.Parse("3c1e4f50-6c2d-4c41-a793-e8e507ab9e77"), me.Id);
        Assert.Equal(Guid.Parse("00d90639-dac7-4eb4-aa13-db2c3e87c874"), me.Account.Id);
        Assert.Equal(Guid.Parse("264e865a-c78b-44a2-a4cb-5991b50db840"), me.Contact.Id);
    }

    private const string LegacyMeResponse = """
        {"id":"3c1e4f50-6c2d-4c41-a793-e8e507ab9e77","role":"owner","archived":false
        ,"user":{"id":"3","name":"Legacy Owner","email":"owner@legacy.example","has_google_registration":false,"first_name":"Legacy"}
        ,"account":{"id":"00d90639-dac7-4eb4-aa13-db2c3e87c874","name":"Legacy LLC","plan_tier":"free","active_templates":0,"can_create_template":true,"can_create_tracking_document":true,"can_create_completion_document":true,"active_users":[{"id":"3","name":"Legacy Owner","email":"owner@legacy.example","has_google_registration":false}]}
        ,"workspace":{"id":"00d90639-dac7-4eb4-aa13-db2c3e87c874","name":"Legacy LLC","plan_tier":"free","active_templates":0,"can_create_template":true,"can_create_tracking_document":true,"can_create_completion_document":true,"active_users":[{"id":"3","name":"Legacy Owner","email":"owner@legacy.example","has_google_registration":false}]}
        ,"contact":{"id":"264e865a-c78b-44a2-a4cb-5991b50db840","email":"owner@legacy.example","name":"Legacy Owner","company_name":"Legacy LLC","phone_number":"970 470 3370","alt_phone_number":"(873) 602-9900","website":"https://legacy.example","initials":"LO","archived":false}}
        """;

    private static SignWellClient Client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) =>
        new(new SignWellClientOptions
        {
            ApiKey = "test-key",
            ConfigureHttpClientBuilder = builder => builder.ConfigurePrimaryHttpMessageHandler(() => new DelegateHandler(send))
        });

    [Fact]
    public async Task RateLimitRetriesReplayJsonRequestsAndThenReturnSuccess()
    {
        var bodies = new List<string>();
        using var client = Client(async (request, cancellationToken) =>
        {
            bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            if (bodies.Count == 1)
            {
                var limited = Json((HttpStatusCode)429, """{"message":"limited"}""");
                limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
                return limited;
            }
            return Json(HttpStatusCode.Created, """{"test_mode":true,"id":"3c1e4f50-6c2d-4c41-a793-e8e507ab9e77"}""");
        });
        var recipient = new RecipientsInner("1", "jane@example.com", "Jane")
        {
            PasscodeDelivery = new RecipientsInnerPasscodeDelivery { Enabled = true, Methods = [RecipientsInnerPasscodeDelivery.MethodsEnum.Email] }
        };
        var request = new DocumentRequest([], [recipient])
        {
            Fields = [[new FieldsInnerInner(1, 2, 1, "1", FieldType.Dropdown) { Options = [new DropdownOption("A"), new DropdownOption(new DetailedOption("B"))] }]]
        };
        var result = await client.CreateDocumentAsync(request);
        Assert.Equal(Guid.Parse("3c1e4f50-6c2d-4c41-a793-e8e507ab9e77"), result.Id);
        Assert.Equal(2, bodies.Count);
        Assert.Equal(bodies[0], bodies[1]);
        using var body = JsonDocument.Parse(bodies[1]);
        Assert.Equal("email", body.RootElement.GetProperty("recipients")[0].GetProperty("passcode_delivery").GetProperty("methods")[0].GetString());
        Assert.Equal("A", body.RootElement.GetProperty("fields")[0][0].GetProperty("options")[0].GetString());
        Assert.Equal("B", body.RootElement.GetProperty("fields")[0][0].GetProperty("options")[1].GetProperty("name").GetString());
    }

    [Fact]
    public async Task RateLimitRetriesStopAtTheLimitAndCanBeDisabled()
    {
        foreach (var retries in new[] { 0, 2 })
        {
            var attempts = 0;
            using var client = new SignWellClient(new SignWellClientOptions
            {
                ApiKey = "test-key",
                MaxRateLimitRetries = retries,
                ConfigureHttpClientBuilder = builder => builder.ConfigurePrimaryHttpMessageHandler(() => new DelegateHandler((_, _) =>
                {
                    attempts++;
                    var limited = Json((HttpStatusCode)429, """{"message":"limited"}""");
                    limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
                    return Task.FromResult(limited);
                }))
            });
            var error = await Assert.ThrowsAsync<RateLimitException>(() => client.Documents.GetAsync(Guid.NewGuid()));
            Assert.Equal((HttpStatusCode)429, error.StatusCode);
            Assert.Equal(retries + 1, attempts);
        }
    }

    [Fact]
    public async Task RateLimitRetryWaitCanBeCancelledWithoutAnotherRequest()
    {
        var attempts = 0;
        var limited = Json((HttpStatusCode)429, "{}");
        limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
        var requested = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = Client((_, _) => { attempts++; requested.TrySetResult(); return Task.FromResult(limited); });
        using var cancellation = new CancellationTokenSource();
        var operation = client.Documents.GetAsync(Guid.NewGuid(), cancellation.Token);
        await requested.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(1, attempts);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => limited.Content.ReadAsStringAsync());
    }

    [Fact]
    public void RateLimitDelaysHonorHeadersAndOtherwiseBackOff()
    {
        var now = DateTimeOffset.UtcNow;
        using var response = Json((HttpStatusCode)429, "{}");
        Assert.Equal(TimeSpan.FromSeconds(1), RateLimitRetryHandler.RetryDelay(response, 0, now));
        Assert.Equal(TimeSpan.FromSeconds(2), RateLimitRetryHandler.RetryDelay(response, 1, now));
        response.Headers.RetryAfter = new RetryConditionHeaderValue(now.AddSeconds(7));
        Assert.Equal(TimeSpan.FromSeconds(7), RateLimitRetryHandler.RetryDelay(response, 0, now));
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120));
        Assert.Null(RateLimitRetryHandler.RetryDelay(response, 0, now));
    }

    [Fact]
    public async Task RateLimitRetriesApplyToDownloadsAndDoNotRetryServerErrors()
    {
        var attempts = 0;
        using var client = Client((_, _) =>
        {
            attempts++;
            if (attempts == 1)
            {
                var limited = Json((HttpStatusCode)429, "{}");
                limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
                return Task.FromResult(limited);
            }
            return Task.FromResult(Binary("application/pdf", "pdf-content"));
        });
        using var downloaded = await client.GetCompletedPdfStreamAsync(Guid.NewGuid());
        Assert.Equal(2, attempts);
        attempts = 0;
        using var failing = Client((_, _) => { attempts++; return Task.FromResult(Json(HttpStatusCode.InternalServerError, "{}")); });
        await Assert.ThrowsAsync<InternalServerException>(() => failing.Documents.GetAsync(Guid.NewGuid()));
        Assert.Equal(1, attempts);
    }

    private static ResponseModesClient Modes(HttpClient client) => new(
        new SingleClientFactory(client),
        new SignWellClientOptions { ApiKey = "test-key" },
        new JsonSerializerOptionsProvider(new JsonSerializerOptions()));

    private static HttpResponseMessage Json(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static HttpResponseMessage Binary(string mediaType, string data)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(data))
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return response;
    }

    private static string Hmac(string key, string value)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private sealed class DelegateHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class TrackingReplayStore : IWebhookReplayStore
    {
        public int Calls { get; private set; }
        public ValueTask<bool> TryAddAsync(string key, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            Calls++;
            return new ValueTask<bool>(true);
        }
    }
}
