using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SignWell.Sdk.Embedded;

public static class EmbeddedRenderer
{
    public const string ScriptUrl = "https://static.signwell.com/assets/embedded.js";
    private static readonly Regex HandlerPath = new(
        @"^[$A-Z_][0-9A-Z_$]*(?:\.[$A-Z_][0-9A-Z_$]*)*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly HashSet<string> BlockedSegments = new(StringComparer.Ordinal)
        { "__proto__", "constructor", "prototype" };

    public static string ScriptTag(string? nonce = null)
    {
        var attribute = string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{WebUtility.HtmlEncode(nonce)}\"";
        return $"<script src=\"{ScriptUrl}\"{attribute}></script>";
    }

    public static string SigningIframe(EmbeddedIframeOptions options) => Render(options, requesting: false);

    public static string RequestingIframe(EmbeddedIframeOptions options) => Render(options, requesting: true);

    private static string Render(EmbeddedIframeOptions options, bool requesting)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        var config = new Dictionary<string, object?>
        {
            ["url"] = ValidateUrl(options.Url, options.AllowedEmbedHosts, "Embed URL")
        };
        Add(config, "containerId", options.ContainerId);
        Add(config, "allowClose", options.AllowClose);
        Add(config, "showHeader", options.ShowHeader);
        Add(config, "allowDownload", options.AllowDownload);
        if (requesting) Add(config, "showSendButton", options.ShowSendButton);
        else Add(config, "allowDecline", options.AllowDecline);
        if (options.RedirectUrl is not null)
            config["redirectUrl"] = ValidateUrl(options.RedirectUrl, options.AllowedRedirectHosts, "Redirect URL");
        if (options.DeclineRedirectUrl is not null)
            config["declineRedirectUrl"] = ValidateUrl(options.DeclineRedirectUrl, options.AllowedRedirectHosts, "Decline redirect URL");

        var events = NormalizeEvents(options.Events);
        var configJson = ScriptSafeJson(config);
        var eventJson = ScriptSafeJson(events);
        var nonce = string.IsNullOrEmpty(options.Nonce) ? string.Empty : $" nonce=\"{WebUtility.HtmlEncode(options.Nonce)}\"";
        var open = options.AutoOpen ? "embed.open();" : string.Empty;
        return $@"<script{nonce}>(function() {{
  var config = {configJson};
  var eventPaths = {eventJson};
  var resolveSignWellHandler = function(path) {{
    if (!path) return null;
    return path.split('.').reduce(function(context, key) {{ return context && context[key]; }}, globalThis);
  }};
  if (Object.keys(eventPaths).length > 0) {{
    config.events = {{}};
    Object.keys(eventPaths).forEach(function(name) {{
      var handler = resolveSignWellHandler(eventPaths[name]);
      if (typeof handler === 'function') config.events[name] = handler;
    }});
  }}
  var embed = new SignWellEmbed(config);
  {open}
}})();</script>";
    }

    private static string ValidateUrl(string raw, IReadOnlyCollection<string>? allowedHosts, string label)
    {
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(uri.Host))
            throw new ArgumentException($"{label} must be an absolute HTTPS URL.");
        if (!string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException($"{label} must not contain credentials.");

        var allowed = NormalizeHosts(allowedHosts);
        var host = uri.IdnHost.ToLowerInvariant();
        var signWell = host == "signwell.com" || host.EndsWith(".signwell.com", StringComparison.Ordinal);
        if (!signWell && !allowed.Contains(host))
            throw new ArgumentException($"{label} host is not allowed.");
        return uri.AbsoluteUri;
    }

    private static HashSet<string> NormalizeHosts(IReadOnlyCollection<string>? hosts)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in hosts ?? Array.Empty<string>())
        {
            var host = raw.Trim().ToLowerInvariant();
            if (host.Length == 0 || host.Contains("/") || host.Contains("@") || host.Contains(":"))
                throw new ArgumentException("Allowed hosts must be exact hostnames.");
            result.Add(host);
        }
        return result;
    }

    private static Dictionary<string, string> NormalizeEvents(IReadOnlyDictionary<string, string>? events)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in events ?? new Dictionary<string, string>())
        {
            var path = pair.Value.Trim();
            if (!HandlerPath.IsMatch(path) || path.Split('.').Any(BlockedSegments.Contains))
                throw new ArgumentException($"Event handler for {pair.Key} must be a safe dotted JavaScript identifier path.");
            result[pair.Key] = path;
        }
        return result;
    }

    private static string ScriptSafeJson<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Default,
            PropertyNamingPolicy = null
        });
        return json
            .Replace("<", "\\u003c")
            .Replace(">", "\\u003e")
            .Replace("&", "\\u0026")
            .Replace("\u2028", "\\u2028")
            .Replace("\u2029", "\\u2029");
    }

    private static void Add(Dictionary<string, object?> target, string key, object? value)
    {
        if (value is not null) target[key] = value;
    }
}
