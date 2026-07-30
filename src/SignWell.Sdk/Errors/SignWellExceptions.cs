using System.Globalization;
using System.Net;
using SignWell.Sdk.Client;
using SignWell.Sdk.Models;

namespace SignWell.Sdk.Errors;

public class SignWellException : Exception
{
    public SignWellException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

public sealed record RateLimitInfo(
    long? Limit,
    long? Remaining,
    long? Reset,
    DateTimeOffset? ResetTime,
    TimeSpan? RetryAfter,
    DateTimeOffset? RetryTime)
{
    internal static RateLimitInfo FromHeaders(HttpResponseMessage response)
    {
        string? First(params string[] names)
        {
            foreach (var name in names)
            {
                if (response.Headers.TryGetValues(name, out var values))
                    return values.FirstOrDefault();
            }
            return null;
        }

        long? Number(params string[] names) =>
            long.TryParse(First(names), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;

        var limit = Number("RateLimit-Limit", "X-RateLimit-Limit");
        var remaining = Number("RateLimit-Remaining", "X-RateLimit-Remaining");
        var reset = Number("RateLimit-Reset", "X-RateLimit-Reset");
        DateTimeOffset? resetTime = null;
        if (reset is not null)
        {
            try { resetTime = DateTimeOffset.FromUnixTimeSeconds(reset.Value); }
            catch (ArgumentOutOfRangeException) { }
        }
        TimeSpan? retryAfter = response.Headers.RetryAfter?.Delta;
        DateTimeOffset? retryTime = response.Headers.RetryAfter?.Date;
        if (retryTime is null && retryAfter is not null)
            retryTime = DateTimeOffset.UtcNow.Add(retryAfter.Value);
        return new RateLimitInfo(limit, remaining, reset, resetTime, retryAfter, retryTime);
    }

    internal static RateLimitInfo FromApiResponse(IApiResponse response)
    {
        using var message = new HttpResponseMessage(response.StatusCode);
        foreach (var header in response.Headers)
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        foreach (var header in response.ContentHeaders)
            message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return FromHeaders(message);
    }
}

public class ApiStatusException : SignWellException
{
    public ApiStatusException(
        HttpStatusCode statusCode,
        object? response,
        string? responseBody,
        IReadOnlyDictionary<string, IReadOnlyList<string>> headers,
        RateLimitInfo rateLimit)
        : base($"SignWell returned HTTP {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        Response = response;
        ResponseBody = responseBody;
        Headers = headers;
        RateLimit = rateLimit;
    }

    public HttpStatusCode StatusCode { get; }
    public object? Response { get; }
    public string? ResponseBody { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }
    public RateLimitInfo RateLimit { get; }
}

public class BadRequestException : ApiStatusException
{
    internal BadRequestException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class AuthenticationException : ApiStatusException
{
    internal AuthenticationException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class ForbiddenException : ApiStatusException
{
    internal ForbiddenException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class PermissionDeniedException : ForbiddenException
{
    internal PermissionDeniedException(ApiErrorContext c) : base(c) { }
}

public class NotFoundException : ApiStatusException
{
    internal NotFoundException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class ConflictException : ApiStatusException
{
    internal ConflictException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class UnprocessableEntityException : ApiStatusException
{
    internal UnprocessableEntityException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class RateLimitException : ApiStatusException
{
    internal RateLimitException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class InternalServerException : ApiStatusException
{
    internal InternalServerException(ApiErrorContext c) : base(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit) { }
}

public class TransportException : SignWellException
{
    public TransportException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class ApiConnectionException : TransportException
{
    public ApiConnectionException(HttpRequestException innerException)
        : base("Could not connect to SignWell.", innerException) { }
}

public sealed class ApiTimeoutException : TransportException
{
    public ApiTimeoutException(Exception innerException)
        : base("The SignWell request timed out.", innerException) { }
}

public sealed class UnsupportedContentTypeException : SignWellException
{
    public UnsupportedContentTypeException(string? actual, string expected)
        : base($"SignWell returned an unsupported content type; expected {expected}.")
    {
        ActualContentType = actual;
        ExpectedContentType = expected;
    }

    public string? ActualContentType { get; }
    public string ExpectedContentType { get; }
}

public sealed class WaitForCompletionTimeoutException : SignWellException
{
    public WaitForCompletionTimeoutException(DocumentResponse? lastResponse)
        : base("The document did not reach a terminal status before the polling limit.")
    {
        LastResponse = lastResponse;
    }

    public DocumentResponse? LastResponse { get; }
}

public class WebhookVerificationException : SignWellException
{
    public WebhookVerificationException(string message) : base(message) { }
}

public sealed class WebhookReplayCapacityException : WebhookVerificationException
{
    public WebhookReplayCapacityException() : base("Webhook replay protection capacity is exhausted.") { }
}

internal sealed record ApiErrorContext(
    HttpStatusCode StatusCode,
    object? Response,
    string? Body,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Headers,
    RateLimitInfo RateLimit);

internal static class ErrorMapper
{
    internal static void ThrowIfError(IApiResponse response, object? parsed = null)
    {
        if (response.IsSuccessStatusCode)
            return;

        var headers = response.Headers
            .Concat(response.ContentHeaders)
            .GroupBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.SelectMany(x => x.Value).ToArray(), StringComparer.OrdinalIgnoreCase);
        parsed ??= TryParseKnownResponse(response);
        var context = new ApiErrorContext(
            response.StatusCode,
            parsed,
            response.RawContent,
            headers,
            RateLimitInfo.FromApiResponse(response));
        throw Create(context);
    }

    internal static async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        cancellationToken.ThrowIfCancellationRequested();
        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.SelectMany(x => x.Value).ToArray(), StringComparer.OrdinalIgnoreCase);
        object? parsed = null;
        if (response.Content.Headers.ContentType?.MediaType is { } mediaType &&
            (mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
             mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(body);
                parsed = document.RootElement.Clone();
            }
            catch (System.Text.Json.JsonException) { }
        }
        var context = new ApiErrorContext(response.StatusCode, parsed, body, headers, RateLimitInfo.FromHeaders(response));
        throw Create(context);
    }

    private static object? TryParseKnownResponse(IApiResponse response)
    {
        var method = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "BadRequest",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "NotFound",
            HttpStatusCode.Conflict => "Conflict",
            (HttpStatusCode)422 => "UnprocessableContent",
            (HttpStatusCode)429 => "TooManyRequests",
            _ => null
        };
        if (method is null) return null;
        try { return response.GetType().GetMethod(method, Type.EmptyTypes)?.Invoke(response, null); }
        catch (Exception) { return null; }
    }

    private static ApiStatusException Create(ApiErrorContext c) => c.StatusCode switch
    {
        HttpStatusCode.BadRequest => new BadRequestException(c),
        HttpStatusCode.Unauthorized => new AuthenticationException(c),
        HttpStatusCode.Forbidden => new PermissionDeniedException(c),
        HttpStatusCode.NotFound => new NotFoundException(c),
        HttpStatusCode.Conflict => new ConflictException(c),
        (HttpStatusCode)422 => new UnprocessableEntityException(c),
        (HttpStatusCode)429 => new RateLimitException(c),
        >= HttpStatusCode.InternalServerError => new InternalServerException(c),
        _ => new ApiStatusException(c.StatusCode, c.Response, c.Body, c.Headers, c.RateLimit)
    };
}
