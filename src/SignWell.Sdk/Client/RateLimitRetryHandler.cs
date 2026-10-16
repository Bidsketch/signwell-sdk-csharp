using System.Net;

namespace SignWell.Sdk.Client;

internal sealed class RateLimitRetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;

    internal RateLimitRetryHandler(int maxRetries) => _maxRetries = maxRetries;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_maxRetries > 0 && request.Content is not null)
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != (HttpStatusCode)429 || attempt >= _maxRetries)
                return response;
            var delay = RetryDelay(response, attempt, DateTimeOffset.UtcNow);
            if (delay is null) return response;
            response.Dispose();
            await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    internal static TimeSpan? RetryDelay(HttpResponseMessage response, int attempt, DateTimeOffset now)
    {
        var retryAfter = response.Headers.RetryAfter;
        var delay = retryAfter?.Delta ?? (retryAfter?.Date is DateTimeOffset date
            ? date - now : TimeSpan.FromSeconds(Math.Pow(2, Math.Min(attempt, 5))));
        if (delay > TimeSpan.FromMinutes(1)) return null;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }
}
