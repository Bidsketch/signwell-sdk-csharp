using SignWell.Sdk.Client;
using SignWell.Sdk.Models;
using SignWell.Sdk.Resources;

namespace SignWell.Sdk.Embedded;

public sealed class EmbeddedWorkflows
{
    private readonly DocumentsResource _documents;
    internal EmbeddedWorkflows(DocumentsResource documents) => _documents = documents;

    public Task<DocumentResponse> CreateSigningDocumentAsync(
        string name,
        IReadOnlyCollection<EmbeddedFile> files,
        IReadOnlyCollection<EmbeddedRecipient> recipients,
        IReadOnlyCollection<IReadOnlyCollection<EmbeddedField>>? fields = null,
        bool withSignaturePage = false,
        bool textTags = false,
        bool testMode = false,
        bool sendNotifications = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Document name is required.", nameof(name));
        var recipientModels = EmbeddedModelFactory.Recipients(recipients);
        var fieldModels = fields is null ? null : EmbeddedModelFactory.Fields(fields, recipientModels);
        EmbeddedModelFactory.ValidatePlacement(recipientModels, fieldModels, withSignaturePage, textTags);
        var request = new DocumentRequest(
            EmbeddedModelFactory.Files(files),
            recipientModels,
            testMode: testMode,
            name: name,
            withSignaturePage: withSignaturePage,
            embeddedSigning: true,
            embeddedSigningNotifications: sendNotifications,
            textTags: textTags,
            fields: fieldModels is null ? default : new Option<List<List<FieldsInnerInner>>?>(fieldModels));
        return _documents.CreateAsync(request, cancellationToken);
    }

    public Task<DocumentResponse> CreateRequestingDocumentAsync(
        string name,
        IReadOnlyCollection<EmbeddedFile> files,
        IReadOnlyCollection<EmbeddedRecipient> recipients,
        bool testMode = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Document name is required.", nameof(name));
        var request = new DocumentRequest(
            EmbeddedModelFactory.Files(files),
            EmbeddedModelFactory.Recipients(recipients),
            testMode: testMode,
            name: name,
            draft: true);
        return _documents.CreateAsync(request, cancellationToken);
    }

    public Task<DocumentFromTemplateResponse> CreateSigningDocumentFromTemplateAsync(
        EmbeddedTemplateSelector selector,
        IReadOnlyCollection<EmbeddedTemplateRecipient> recipients,
        bool testMode = false,
        bool sendNotifications = false,
        CancellationToken cancellationToken = default)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        selector.Validate();
        var request = new DocumentFromTemplateRequest(
            EmbeddedModelFactory.TemplateRecipients(recipients),
            testMode: testMode,
            templateId: selector.TemplateId.HasValue ? selector.TemplateId.Value : default,
            templateIds: selector.TemplateIds is { Count: > 0 }
                ? selector.TemplateIds.ToList()
                : default,
            embeddedSigning: true,
            embeddedSigningNotifications: sendNotifications);
        return _documents.CreateFromTemplateAsync(request, cancellationToken);
    }

    public static string? SigningUrl(DocumentResponse response, int recipientIndex = 0) =>
        response.Recipients is { } recipients && recipientIndex >= 0 && recipientIndex < recipients.Count
            ? recipients[recipientIndex].EmbeddedSigningUrl
            : null;

    public static IReadOnlyDictionary<string, string> SigningUrls(DocumentResponse response) =>
        (response.Recipients ?? new List<DocumentResponseRecipientsInner>())
            .Where(r => !string.IsNullOrWhiteSpace(r.Email) && !string.IsNullOrWhiteSpace(r.EmbeddedSigningUrl))
            .ToDictionary(r => r.Email, r => r.EmbeddedSigningUrl!, StringComparer.OrdinalIgnoreCase);

    public static string? SigningUrl(DocumentFromTemplateResponse response, int recipientIndex = 0) =>
        response.Recipients is { } recipients && recipientIndex >= 0 && recipientIndex < recipients.Count
            ? recipients[recipientIndex].EmbeddedSigningUrl
            : null;

    public static IReadOnlyDictionary<string, string> SigningUrls(DocumentFromTemplateResponse response) =>
        (response.Recipients ?? new List<DocumentFromTemplateResponseRecipientsInner>())
            .Where(r => !string.IsNullOrWhiteSpace(r.Email) && !string.IsNullOrWhiteSpace(r.EmbeddedSigningUrl))
            .ToDictionary(r => r.Email, r => r.EmbeddedSigningUrl!, StringComparer.OrdinalIgnoreCase);
}
