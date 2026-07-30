using Microsoft.Extensions.DependencyInjection;

namespace SignWell.Sdk;

public sealed class SignWellClientOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public Uri BaseUri { get; set; } = new("https://www.signwell.com", UriKind.Absolute);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public Action<IHttpClientBuilder>? ConfigureHttpClientBuilder { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("A nonblank API key is required.", nameof(ApiKey));
        if (Timeout <= TimeSpan.Zero && Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(Timeout), "Timeout must be positive or infinite.");

        BaseUri = NormalizeBaseUri(BaseUri);
    }

    internal static Uri NormalizeBaseUri(Uri uri)
    {
        if (uri is null) throw new ArgumentNullException(nameof(uri));
        if (!uri.IsAbsoluteUri || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The base URI must be an absolute HTTPS URL.", nameof(uri));
        if (!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("The base URI cannot contain credentials, a query string, or a fragment.", nameof(uri));

        var builder = new UriBuilder(uri) { Path = uri.AbsolutePath.TrimEnd('/') };
        return builder.Uri;
    }
}
