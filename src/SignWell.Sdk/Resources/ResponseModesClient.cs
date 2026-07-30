using System.Net.Http.Headers;
using System.Text.Json;
using SignWell.Sdk.Client;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;

namespace SignWell.Sdk.Resources;

public sealed class OwnedResponseStream : Stream
{
    private readonly HttpResponseMessage _response;
    private readonly Stream _inner;
    private bool _disposed;

    internal OwnedResponseStream(HttpResponseMessage response, Stream inner)
    {
        _response = response;
        _inner = inner;
    }

    public HttpResponseHeaders Headers => _response.Headers;
    public HttpContentHeaders ContentHeaders => _response.Content.Headers;
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _inner.Dispose();
            _response.Dispose();
        }
        base.Dispose(disposing);
    }
}

public sealed class ResponseModesClient
{
    internal const string HttpClientName = "SignWell.Sdk.ResponseModes";
    private readonly IHttpClientFactory _factory;
    private readonly SignWellClientOptions _options;
    private readonly JsonSerializerOptions _json;

    public ResponseModesClient(
        IHttpClientFactory factory,
        SignWellClientOptions options,
        JsonSerializerOptionsProvider json)
    {
        _factory = factory;
        _options = options;
        _json = json.Options;
    }

    public Task<OwnedResponseStream> GetCompletedPdfStreamAsync(
        Guid id, bool auditPage = false, FileFormat? fileFormat = null, CancellationToken cancellationToken = default) =>
        SendStreamAsync(
            $"/api/v1/documents/{id:D}/completed_pdf?url_only=false&audit_page={Lower(auditPage)}{FileFormatQuery(fileFormat)}",
            "PDF or ZIP content",
            cancellationToken);

    public Task<CompletedPdfUrlResponse> GetCompletedPdfUrlAsync(
        Guid id, bool auditPage = false, FileFormat? fileFormat = null, CancellationToken cancellationToken = default) =>
        SendJsonAsync<CompletedPdfUrlResponse>(
            $"/api/v1/documents/{id:D}/completed_pdf?url_only=true&audit_page={Lower(auditPage)}{FileFormatQuery(fileFormat)}",
            cancellationToken);

    public Task<OwnedResponseStream> GetBulkSendCsvStreamAsync(
        IReadOnlyCollection<Guid> templateIds, CancellationToken cancellationToken = default) =>
        SendStreamAsync(BulkCsvPath(templateIds, false), "CSV content", cancellationToken);

    public Task<BulkSendCsvTemplateResponse> GetBulkSendCsvBase64Async(
        IReadOnlyCollection<Guid> templateIds, CancellationToken cancellationToken = default) =>
        SendJsonAsync<BulkSendCsvTemplateResponse>(BulkCsvPath(templateIds, true), cancellationToken);

    public Task<OwnedResponseStream> GetNom151StreamAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendStreamAsync($"/api/v1/documents/{id:D}/nom151_certificate?url_only=false&object_only=false", "ZIP content", cancellationToken);

    public Task<Nom151UrlResponse> GetNom151UrlAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendJsonAsync<Nom151UrlResponse>($"/api/v1/documents/{id:D}/nom151_certificate?url_only=true&object_only=false", cancellationToken);

    public Task<Nom151CertificateResponse> GetNom151ObjectAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendJsonAsync<Nom151CertificateResponse>($"/api/v1/documents/{id:D}/nom151_certificate?url_only=false&object_only=true", cancellationToken);

    private async Task<OwnedResponseStream> SendStreamAsync(string path, string expected, CancellationToken cancellationToken)
    {
        var response = await SendAsync(path, cancellationToken).ConfigureAwait(false);
        try
        {
            await ErrorMapper.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!IsBinary(mediaType))
                throw new UnsupportedContentTypeException(mediaType, expected);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new OwnedResponseStream(response, stream);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private async Task<T> SendJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(path, cancellationToken).ConfigureAwait(false);
        await ErrorMapper.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null || !(mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
                                   mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
            throw new UnsupportedContentTypeException(mediaType, "JSON");
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, _json, cancellationToken).ConfigureAwait(false)
            ?? throw new SignWellException("SignWell returned an empty JSON response.");
    }

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken cancellationToken)
    {
        var client = _factory.CreateClient(HttpClientName);
        client.DefaultRequestHeaders.Remove("X-Api-Key");
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Api-Key", _options.ApiKey);
        try
        {
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiTimeoutException(ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException(ex);
        }
    }

    private static string BulkCsvPath(IReadOnlyCollection<Guid> ids, bool base64)
    {
        if (ids is null || ids.Count == 0)
            throw new ArgumentException("At least one template ID is required.", nameof(ids));
        var values = string.Join(",", ids.Select(id => Uri.EscapeDataString(id.ToString("D"))));
        return $"/api/v1/bulk_sends/csv_template?template_ids={values}&base64={Lower(base64)}";
    }

    private static string Lower(bool value) => value ? "true" : "false";

    private static string FileFormatQuery(FileFormat? value) =>
        value is null ? string.Empty : $"&file_format={Uri.EscapeDataString(FileFormatValueConverter.ToJsonValue(value.Value))}";

    private static bool IsBinary(string? mediaType) =>
        mediaType is not null && (
            mediaType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/zip", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("text/csv", StringComparison.OrdinalIgnoreCase));
}
