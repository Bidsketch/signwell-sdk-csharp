using SignWell.Sdk.Models;

namespace SignWell.Sdk.Embedded;

public sealed record EmbeddedRecipient(string Name, string Email, string? Id = null, object? Passcode = null);

public sealed record EmbeddedTemplateRecipient(
    string Name,
    string Email,
    string? PlaceholderName = null,
    string? Id = null,
    object? Passcode = null);

public sealed record EmbeddedFile(string Name, string? FileUrl = null, string? FileBase64 = null);

public sealed record EmbeddedField(
    float X,
    float Y,
    int Page,
    FieldType Type,
    string? RecipientId = null,
    bool Required = true,
    string? Label = null);

public sealed record EmbeddedIframeOptions(
    string Url,
    IReadOnlyCollection<string>? AllowedEmbedHosts = null,
    IReadOnlyCollection<string>? AllowedRedirectHosts = null,
    string? ContainerId = null,
    bool? AllowDecline = null,
    bool? AllowClose = null,
    bool? ShowHeader = null,
    bool? AllowDownload = null,
    bool? ShowSendButton = null,
    string? RedirectUrl = null,
    string? DeclineRedirectUrl = null,
    IReadOnlyDictionary<string, string>? Events = null,
    bool AutoOpen = true,
    string? Nonce = null);

public sealed record EmbeddedTemplateSelector(Guid? TemplateId = null, IReadOnlyCollection<Guid>? TemplateIds = null)
{
    public void Validate()
    {
        var single = TemplateId.HasValue;
        var many = TemplateIds is { Count: > 0 };
        if (single == many)
            throw new ArgumentException("Provide exactly one template ID or one non-empty collection of template IDs.");
    }
}
