using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SignWell.Sdk;

public sealed class SignWellClientOptions
{
    private const string FallbackSdkVersion = "0.2.0-beta.1";

    /// <summary>
    /// Default User-Agent sent by the SDK, derived from the package version so a version
    /// bump cannot leave a stale string behind.
    /// </summary>
    public static readonly string UserAgent = "signwell-sdk-csharp/" + SdkVersion();

    public string ApiKey { get; set; } = string.Empty;

    public Uri BaseUri { get; set; } = new("https://www.signwell.com", UriKind.Absolute);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Retries after an explicit HTTP 429 response. Set to zero to disable.</summary>
    public int MaxRateLimitRetries { get; set; } = 2;

    public Action<IHttpClientBuilder>? ConfigureHttpClientBuilder { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("A nonblank API key is required.", nameof(ApiKey));
        if (Timeout <= TimeSpan.Zero && Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(Timeout), "Timeout must be positive or infinite.");
        if (MaxRateLimitRetries < 0 || MaxRateLimitRetries > 10)
            throw new ArgumentOutOfRangeException(nameof(MaxRateLimitRetries), "Rate limit retries must be between zero and ten.");

        BaseUri = NormalizeBaseUri(BaseUri);
    }

    private static string SdkVersion()
    {
        var informational = typeof(SignWellClientOptions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (informational is null || informational.Trim().Length == 0) return FallbackSdkVersion;

        // SourceLink appends "+<commit sha>" to the informational version; the User-Agent
        // carries the package version only.
        var separator = informational.IndexOf('+');
        return separator < 0 ? informational : informational.Substring(0, separator);
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
