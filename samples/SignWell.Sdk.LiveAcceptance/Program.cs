using SignWell.Sdk;
using SignWell.Sdk.Embedded;
using SignWell.Sdk.Errors;

static string Required(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"{name} is required.");

var apiKey = Required("SIGNWELL_API_KEY");
var documentId = Guid.Parse(Required("SIGNWELL_COMPLETED_DOCUMENT_ID"));
var nom151DocumentId = Guid.Parse(Required("SIGNWELL_NOM151_DOCUMENT_ID"));
var templateIds = Required("SIGNWELL_TEMPLATE_IDS").Split(',').Select(Guid.Parse).ToArray();
var fixtureFileUrl = Required("SIGNWELL_FIXTURE_FILE_URL");
var recipientEmail = Required("SIGNWELL_RECIPIENT_EMAIL");

using var client = new SignWellClient(apiKey);
var me = await client.GetMeAsync();
Console.WriteLine($"Account lookup: {me.GetType().Name}");

await foreach (var page in client.Documents.GetPagesAsync(limit: 2))
{
    Console.WriteLine($"Document page: {page.CurrentPage}");
    break;
}
await foreach (var page in client.Templates.GetPagesAsync(limit: 2))
{
    Console.WriteLine($"Template page: {page.CurrentPage}");
    break;
}

using (var pdf = await client.Documents.GetCompletedPdfStreamAsync(documentId))
    Console.WriteLine($"Completed PDF first byte: {pdf.ReadByte()}");
var pdfUrl = await client.Documents.GetCompletedPdfUrlAsync(documentId);
Console.WriteLine($"Completed PDF URL: {new Uri(pdfUrl.FileUrl).Host}");

using (var csv = await client.BulkSends.GetCsvTemplateStreamAsync(templateIds))
    Console.WriteLine($"Bulk CSV first byte: {csv.ReadByte()}");
var csvBase64 = await client.BulkSends.GetCsvTemplateBase64Async(templateIds);
Console.WriteLine($"Bulk CSV base64 bytes: {csvBase64.Data.Length}");

using (var certificate = await client.Regional.GetNom151StreamAsync(nom151DocumentId))
    Console.WriteLine($"NOM-151 first byte: {certificate.ReadByte()}");
Console.WriteLine($"NOM-151 URL host: {new Uri((await client.Regional.GetNom151UrlAsync(nom151DocumentId)).FileUrl).Host}");
Console.WriteLine($"NOM-151 object: {(await client.Regional.GetNom151ObjectAsync(nom151DocumentId)).GetType().Name}");

var created = new List<Guid>();
try
{
    var draft = await client.Embedded.CreateRequestingDocumentAsync(
        "C# SDK live acceptance draft",
        [new EmbeddedFile("fixture.pdf", FileUrl: fixtureFileUrl)],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        testMode: true);
    created.Add(draft.Id);
    Console.WriteLine($"Draft created: {draft.Id}");

    var signing = await client.Embedded.CreateSigningDocumentAsync(
        "C# SDK live acceptance embedded signing",
        [new EmbeddedFile("fixture.pdf", FileUrl: fixtureFileUrl)],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        withSignaturePage: true,
        testMode: true);
    created.Add(signing.Id);
    Console.WriteLine($"Embedded URL present: {EmbeddedWorkflows.SigningUrl(signing) is not null}");

    try
    {
        await client.Documents.GetAsync(Guid.NewGuid());
        throw new InvalidOperationException("Expected a typed not-found failure.");
    }
    catch (NotFoundException)
    {
        Console.WriteLine("Typed failure mapping: NotFoundException");
    }
}
finally
{
    foreach (var id in created)
    {
        try { await client.Documents.DeleteAsync(id); }
        catch (SignWellException exception) { Console.Error.WriteLine($"Cleanup failed for {id}: {exception.GetType().Name}"); }
    }
}
