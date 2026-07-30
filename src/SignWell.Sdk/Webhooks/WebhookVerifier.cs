using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SignWell.Sdk.Errors;

namespace SignWell.Sdk.Webhooks;

public interface IWebhookReplayStore
{
    ValueTask<bool> TryAddAsync(string key, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
}

public sealed class InMemoryWebhookReplayStore : IWebhookReplayStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, DateTimeOffset> _entries = new(StringComparer.Ordinal);
    private readonly int _capacity;
    private readonly Func<DateTimeOffset> _clock;

    public InMemoryWebhookReplayStore(int capacity = 10_000, Func<DateTimeOffset>? clock = null)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public ValueTask<bool> TryAddAsync(string key, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            var now = _clock();
            foreach (var expired in _entries.Where(x => x.Value <= now).Select(x => x.Key).ToArray())
                _entries.Remove(expired);
            if (_entries.ContainsKey(key))
                return new ValueTask<bool>(false);
            if (_entries.Count >= _capacity)
                throw new WebhookReplayCapacityException();
            _entries.Add(key, expiresAt);
            return new ValueTask<bool>(true);
        }
    }
}

public static class WebhookVerifier
{
    public static bool Verify(
        JsonElement eventData,
        string webhookId,
        TimeSpan? freshness = null,
        DateTimeOffset? now = null)
    {
        try
        {
            VerifyOrThrow(eventData, webhookId, freshness, now);
            return true;
        }
        catch (WebhookVerificationException)
        {
            return false;
        }
    }

    public static void VerifyOrThrow(
        JsonElement eventData,
        string webhookId,
        TimeSpan? freshness = null,
        DateTimeOffset? now = null) =>
        VerifyData(eventData, webhookId, freshness, now ?? DateTimeOffset.UtcNow);

    public static async ValueTask<bool> VerifyOnceAsync(
        JsonElement eventData,
        string webhookId,
        IWebhookReplayStore replayStore,
        TimeSpan freshness,
        DateTimeOffset? now = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await VerifyOnceOrThrowAsync(eventData, webhookId, replayStore, freshness, now, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (WebhookVerificationException)
        {
            return false;
        }
    }

    public static async ValueTask VerifyOnceOrThrowAsync(
        JsonElement eventData,
        string webhookId,
        IWebhookReplayStore replayStore,
        TimeSpan freshness,
        DateTimeOffset? now = null,
        CancellationToken cancellationToken = default)
    {
        if (replayStore is null) throw new ArgumentNullException(nameof(replayStore));
        var current = now ?? DateTimeOffset.UtcNow;
        var parsed = VerifyData(eventData, webhookId, freshness, current);
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(parsed.Timestamp).Add(freshness);
        if (!await replayStore.TryAddAsync(parsed.ReplayKey, expiresAt, cancellationToken).ConfigureAwait(false))
            throw new WebhookVerificationException("Webhook event has already been processed.");
    }

    public static string ReplayKey(JsonElement eventData)
    {
        var parsed = Parse(eventData);
        return $"signwell:{parsed.Type}:{parsed.TimeText}:{parsed.Hash}";
    }

    private static ParsedEvent VerifyData(JsonElement eventData, string webhookId, TimeSpan? freshness, DateTimeOffset now)
    {
        if (string.IsNullOrEmpty(webhookId))
            throw new WebhookVerificationException("Webhook ID must be a non-empty string.");
        var parsed = Parse(eventData);
        if (freshness is not null)
        {
            if (freshness < TimeSpan.Zero)
                throw new WebhookVerificationException("Freshness must be non-negative.");
            var delta = Math.Abs(now.ToUnixTimeSeconds() - parsed.Timestamp);
            if (delta > freshness.Value.TotalSeconds)
                throw new WebhookVerificationException("Webhook timestamp is outside the allowed freshness window.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookId));
        var calculated = ToLowerHex(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{parsed.Type}@{parsed.TimeText}")));
        if (!ConstantTimeEquals(calculated, parsed.Hash))
            throw new WebhookVerificationException("Webhook signature is invalid.");
        return parsed;
    }

    private static ParsedEvent Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw new WebhookVerificationException("Webhook event must be a JSON object.");
        if (!value.TryGetProperty("type", out var type) || !value.TryGetProperty("time", out var time) ||
            !value.TryGetProperty("hash", out var hash))
            throw new WebhookVerificationException("Webhook event requires type, time, and hash. Pass the event object, not the full payload.");
        var typeText = ScalarText(type);
        var timeText = ScalarText(time);
        if (hash.ValueKind != JsonValueKind.String)
            throw new WebhookVerificationException("Webhook event hash must be a string.");
        var hashText = hash.GetString();
        if (string.IsNullOrEmpty(typeText) || string.IsNullOrEmpty(timeText) || string.IsNullOrEmpty(hashText))
            throw new WebhookVerificationException("Webhook event type, time, and hash cannot be empty.");
        if (!long.TryParse(timeText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var timestamp))
            throw new WebhookVerificationException("Webhook event time must be a Unix timestamp.");
        return new ParsedEvent(typeText, timeText, hashText!, timestamp);
    }

    private static string ScalarText(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        _ => string.Empty
    };

    private static bool ConstantTimeEquals(string left, string right)
    {
        var a = Encoding.UTF8.GetBytes(left);
        var b = Encoding.UTF8.GetBytes(right.ToLowerInvariant());
        var length = Math.Max(a.Length, b.Length);
        var difference = a.Length ^ b.Length;
        for (var i = 0; i < length; i++)
            difference |= (i < a.Length ? a[i] : 0) ^ (i < b.Length ? b[i] : 0);
        return difference == 0;
    }

    private static string ToLowerHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var value in bytes) builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        return builder.ToString();
    }

    private sealed record ParsedEvent(string Type, string TimeText, string Hash, long Timestamp)
    {
        internal string ReplayKey => $"signwell:{Type}:{TimeText}:{Hash}";
    }
}
