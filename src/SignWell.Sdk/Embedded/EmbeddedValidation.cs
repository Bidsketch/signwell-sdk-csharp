using System.Globalization;
using System.Text;
using SignWell.Sdk.Client;
using SignWell.Sdk.Models;

namespace SignWell.Sdk.Embedded;

public static class SignWellValues
{
    public static string NormalizeCheckbox(object value)
    {
        if (value is bool boolean)
            return boolean ? "t" : "f";
        if (value is string text)
        {
            return text.Trim().ToLowerInvariant() switch
            {
                "true" or "t" => "t",
                "false" or "f" => "f",
                _ => throw new ArgumentException("Checkbox values must be true, false, \"true\", \"false\", \"t\", or \"f\".", nameof(value))
            };
        }
        throw new ArgumentException("Checkbox values must be booleans or recognized boolean strings.", nameof(value));
    }

    public static string? NormalizePasscode(object? value)
    {
        if (value is null) return null;
        var result = Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim();
        return string.IsNullOrEmpty(result) ? null : result;
    }
}

internal static class EmbeddedModelFactory
{
    internal static List<FilesInner> Files(IReadOnlyCollection<EmbeddedFile> files)
    {
        if (files is null || files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));
        return files.Select(File).ToList();
    }

    internal static List<RecipientsInner> Recipients(IReadOnlyCollection<EmbeddedRecipient> recipients)
    {
        if (recipients is null || recipients.Count == 0)
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        return recipients.Select((r, i) =>
        {
            RequireIdentity(r.Name, r.Email);
            return new RecipientsInner(
                r.Id ?? (i + 1).ToString(CultureInfo.InvariantCulture),
                r.Email,
                name: r.Name,
                passcode: SignWellValues.NormalizePasscode(r.Passcode));
        }).ToList();
    }

    internal static List<TemplateRecipientsInner> TemplateRecipients(IReadOnlyCollection<EmbeddedTemplateRecipient> recipients)
    {
        if (recipients is null || recipients.Count == 0)
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        return recipients.Select((r, i) =>
        {
            RequireIdentity(r.Name, r.Email);
            return new TemplateRecipientsInner(
                r.Id ?? (i + 1).ToString(CultureInfo.InvariantCulture),
                r.Email,
                name: r.Name,
                placeholderName: r.PlaceholderName,
                passcode: SignWellValues.NormalizePasscode(r.Passcode));
        }).ToList();
    }

    internal static List<List<FieldsInnerInner>> Fields(
        IReadOnlyCollection<IReadOnlyCollection<EmbeddedField>> fields,
        IReadOnlyList<RecipientsInner> recipients)
    {
        var defaultId = recipients.FirstOrDefault()?.Id;
        return fields.Select(fileFields => fileFields.Select(field =>
        {
            var recipientId = field.RecipientId ?? defaultId;
            if (string.IsNullOrWhiteSpace(recipientId))
                throw new ArgumentException("Every field must identify a recipient.");
            return new FieldsInnerInner(
                field.X, field.Y, field.Page, recipientId!, field.Type,
                required: field.Required, label: field.Label);
        }).ToList()).ToList();
    }

    internal static void ValidatePlacement(
        IReadOnlyList<RecipientsInner> recipients,
        IReadOnlyList<List<FieldsInnerInner>>? fields,
        bool withSignaturePage,
        bool textTags)
    {
        if (withSignaturePage || textTags) return;
        var assigned = new HashSet<string>(
            (fields ?? Array.Empty<List<FieldsInnerInner>>()).SelectMany(x => x).Select(x => x.RecipientId),
            StringComparer.Ordinal);
        if (assigned.Count == 0 || recipients.Any(r => !assigned.Contains(r.Id)))
            throw new ArgumentException("Embedded signing requires fields for every recipient, signature-page mode, or text-tag mode.");
    }

    private static FilesInner File(EmbeddedFile input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("Every file requires a name.", nameof(input));
        var hasUrl = !string.IsNullOrWhiteSpace(input.FileUrl);
        var hasBase64 = !string.IsNullOrWhiteSpace(input.FileBase64);
        if (hasUrl == hasBase64)
            throw new ArgumentException("Every file must contain exactly one of FileUrl or FileBase64.", nameof(input));
        if (hasUrl)
            return new FilesInner(input.Name, fileUrl: input.FileUrl);

        var compact = new string(input.FileBase64!.Where(c => !char.IsWhiteSpace(c)).ToArray());
        byte[] bytes;
        try { bytes = Convert.FromBase64String(compact); }
        catch (FormatException ex) { throw new ArgumentException("FileBase64 must contain valid RFC 4648 base64.", nameof(input), ex); }
        if (input.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            ValidatePdf(bytes);
        return new FilesInner(input.Name, fileBase64: bytes);
    }

    private static void ValidatePdf(byte[] bytes)
    {
        var header = Encoding.ASCII.GetBytes("%PDF-");
        if (bytes.Length < header.Length || !bytes.Take(header.Length).SequenceEqual(header))
            throw new ArgumentException("PDF content is missing its PDF header.");
        var end = bytes.Length - 1;
        while (end >= 0 && (bytes[end] == 0 || char.IsWhiteSpace((char)bytes[end]))) end--;
        var trailer = Encoding.ASCII.GetBytes("%%EOF");
        if (end + 1 < trailer.Length ||
            !bytes.Skip(end + 1 - trailer.Length).Take(trailer.Length).SequenceEqual(trailer))
            throw new ArgumentException("PDF content is incomplete or missing its EOF trailer.");
    }

    private static void RequireIdentity(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Recipient name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Recipient email is required.");
    }
}
