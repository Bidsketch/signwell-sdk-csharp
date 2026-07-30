using SignWell.Sdk.Errors;

namespace SignWell.Sdk.Resources;

internal static class SdkCall
{
    internal static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        try
        {
            return await action().ConfigureAwait(false);
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
}
