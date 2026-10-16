using SignWell.Sdk;
using SignWell.Sdk.Embedded;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;

static string Required(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"{name} is required.");

static async Task ExpectNotFound(Func<Task> action, string operation)
{
    try
    {
        await action();
        throw new InvalidOperationException($"Expected a typed not-found failure for {operation}.");
    }
    catch (NotFoundException)
    {
    }
}

var apiKey = Required("SIGNWELL_API_KEY");
var recipientEmail = Required("SIGNWELL_RECIPIENT_EMAIL");
var completedDocumentId = Guid.Parse(Required("SIGNWELL_COMPLETED_DOCUMENT_ID"));
var nom151Fixture = Environment.GetEnvironmentVariable("SIGNWELL_NOM151_DOCUMENT_ID");
Guid? nom151DocumentId = string.IsNullOrWhiteSpace(nom151Fixture) ? null : Guid.Parse(nom151Fixture);
var fixtureFileUrl = Environment.GetEnvironmentVariable("SIGNWELL_FIXTURE_FILE_URL")
    ?? "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf";

using var client = new SignWellClient(apiKey);
var documents = new List<Guid>();
var templates = new List<Guid>();
try
{
    var me = await client.GetMeAsync();
    Console.WriteLine($"PASS account lookup: {me.GetType().Name}");

    await foreach (var page in client.Documents.GetPagesAsync(limit: 2))
    {
        Console.WriteLine($"PASS document pagination: {page.CurrentPage}");
        break;
    }
    await foreach (var page in client.Templates.GetPagesAsync(limit: 2))
    {
        Console.WriteLine($"PASS template pagination: {page.CurrentPage}");
        break;
    }
    var existingBulkSends = await client.BulkSends.Raw.ListBulkSendsAsync(limit: 2);
    ErrorCheck(existingBulkSends);
    Console.WriteLine("PASS bulk-send listing");
    var webhooks = await client.Webhooks.ListAsync();
    Console.WriteLine($"PASS webhook listing: {webhooks.Count}");

    var sent = await client.CreateDocumentAsync(new DocumentRequest(
        [new FilesInner("fixture.pdf", fileUrl: fixtureFileUrl)],
        [new RecipientsInner("1", email: recipientEmail, name: "SDK Acceptance")],
        testMode: true,
        name: ".NET SDK live acceptance draft",
        draft: true,
        withSignaturePage: true));
    documents.Add(sent.Id);
    require(sent.Id != Guid.Empty, "Sent document has no ID.");
    require((await client.Documents.GetAsync(sent.Id)).Id == sent.Id, "Sent document lookup failed.");
    sent = await WaitForDraftAsync(client, sent.Id);
    sent = await client.SendDocumentAsync(sent.Id, new UpdateDocumentAndSendRequest());
    sent = await WaitForSentAsync(client, sent.Id);
    var sentRecipientId = sent.Recipients?.FirstOrDefault()?.Id
        ?? throw new InvalidOperationException("Sent document recipient has no ID.");
    require((await client.Documents.UpdateRecipientsAsync(
        sent.Id,
        new UpdateRecipientsRequest([new UpdateRecipientsMapInner(sentRecipientId, "SDK Acceptance Updated", recipientEmail)]))).Id == sent.Id,
        "Recipient update failed.");
    Console.WriteLine("PASS direct create, lookup, recipient update, and send");

    var signing = await client.Embedded.CreateSigningDocumentAsync(
        ".NET SDK live acceptance signature page",
        [new EmbeddedFile("fixture.pdf", FileUrl: fixtureFileUrl)],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        withSignaturePage: true,
        testMode: true);
    documents.Add(signing.Id);
    signing = await WaitForSentAsync(client, signing.Id);
    var signingRecipientId = signing.Recipients?.FirstOrDefault()?.Id
        ?? throw new InvalidOperationException("Signing document recipient has no ID.");
    require(EmbeddedWorkflows.SigningUrl(signing) is not null, "Signing URL is missing.");
    require((await client.Documents.UpdateAuthenticationAsync(
        signing.Id,
        new UpdateAuthenticationRequest([new UpdateAuthenticationRequestRecipientsInner(signingRecipientId, passcode: "1234")]))).Id == signing.Id,
        "Authentication update failed.");
    await client.SendReminderAsync(signing.Id, new SendReminderRequest(
        recipients: new List<ReminderRecipientsInner> { new(name: "SDK Acceptance", email: recipientEmail) }));

    var pdf = await new HttpClient().GetByteArrayAsync(fixtureFileUrl);
    var base64 = await client.Embedded.CreateSigningDocumentAsync(
        ".NET SDK live acceptance base64 PDF",
        [new EmbeddedFile("fixture.pdf", FileBase64: Convert.ToBase64String(pdf))],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        withSignaturePage: true,
        testMode: true);
    documents.Add(base64.Id);
    var fields = await client.Embedded.CreateSigningDocumentAsync(
        ".NET SDK live acceptance fields",
        [new EmbeddedFile("fixture.pdf", FileUrl: fixtureFileUrl)],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        [[new EmbeddedField(72, 520, 1, FieldType.Signature)]],
        testMode: true);
    documents.Add(fields.Id);
    var textTags = await client.Embedded.CreateSigningDocumentAsync(
        ".NET SDK live acceptance text tags",
        [new EmbeddedFile("fixture.pdf", FileUrl: fixtureFileUrl)],
        [new EmbeddedRecipient("SDK Acceptance", recipientEmail)],
        textTags: true,
        testMode: true);
    documents.Add(textTags.Id);
    Console.WriteLine("PASS URL, base64, signature-page, field, text-tag, authentication, and reminder workflows");

    await client.Documents.DeleteAsync(base64.Id);
    documents.Remove(base64.Id);
    Console.WriteLine("PASS document delete");
    await Task.Delay(TimeSpan.FromSeconds(60));

    var firstTemplate = await CreateTemplateAsync(client, templates, fixtureFileUrl, null, "one");
    var secondTemplate = await CreateTemplateAsync(client, templates, null, pdf, "two");
    require((await client.UpdateTemplateAsync(firstTemplate, new DocumentTemplateUpdateRequest(name: "Updated template"))).Id == firstTemplate,
        "Template update failed.");
    require((await client.Templates.GetAsync(firstTemplate)).Id == firstTemplate, "Template lookup failed.");

    var single = await client.Embedded.CreateSigningDocumentFromTemplateAsync(
        new EmbeddedTemplateSelector(TemplateId: firstTemplate),
        [new EmbeddedTemplateRecipient("SDK Acceptance", recipientEmail, PlaceholderName: "Signer")],
        testMode: true);
    documents.Add(single.Id);
    var multiple = await client.Embedded.CreateSigningDocumentFromTemplateAsync(
        new EmbeddedTemplateSelector(TemplateIds: [firstTemplate, secondTemplate]),
        [new EmbeddedTemplateRecipient("SDK Acceptance", recipientEmail, PlaceholderName: "Signer")],
        testMode: true);
    documents.Add(multiple.Id);
    require(EmbeddedWorkflows.SigningUrl(single) is not null, "Single-template signing URL is missing.");
    require(EmbeddedWorkflows.SigningUrl(multiple) is not null, "Multi-template signing URL is missing.");
    Console.WriteLine("PASS template create, update, get, and single/multi-template documents");

    var templateIds = new[] { firstTemplate, secondTemplate };
    using (var csvStream = await client.BulkSends.GetCsvTemplateStreamAsync(templateIds))
        require(csvStream.ReadByte() >= 0, "Bulk-send CSV stream is empty.");
    var csvResponse = await client.BulkSends.GetCsvTemplateBase64Async(templateIds);
    require(csvResponse.Data.Length > 0, "Bulk-send base64 response is empty.");
    var csv = Convert.ToBase64String(csvResponse.Data);
    require((await client.ValidateBulkSendCsvAsync(new BulkSendCsvRequest(templateIds.ToList(), csv))) is not null,
        "Bulk-send validation returned null.");
    try
    {
        await client.CreateBulkSendAsync(new CreateBulkSendRequest(templateIds.ToList(), Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-a-valid-csv"))));
        throw new InvalidOperationException("Expected invalid bulk-send CSV failure.");
    }
    catch (UnprocessableEntityException)
    {
    }
    var bulkList = await client.BulkSends.Raw.ListBulkSendsAsync(limit: 50);
    ErrorCheck(bulkList);
    if (bulkList.Ok()?.BulkSends is { Count: > 0 } listed)
    {
        var bulkId = listed[0].Id;
        require((await client.BulkSends.GetAsync(bulkId)).Id == bulkId, "Bulk-send lookup failed.");
        require((await client.BulkSends.Raw.GetBulkSendDocumentsAsync(bulkId, limit: 50, page: 1)).Ok() is not null,
            "Bulk-send documents lookup failed.");
    }
    else
    {
        await ExpectNotFound(() => client.BulkSends.GetAsync(Guid.NewGuid()), "bulk-send lookup");
        await ExpectNotFound(async () => ErrorCheck(await client.BulkSends.Raw.GetBulkSendDocumentsAsync(Guid.NewGuid(), limit: 50, page: 1)), "bulk-send documents lookup");
    }
    var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest("https://example.com/signwell-sdk-live-webhook"));
    require((await client.Webhooks.ListAsync()).Any(item => item.Id == webhook.Id), "Webhook listing missed created webhook.");
    await client.Webhooks.DeleteAsync(webhook.Id);
    Console.WriteLine("PASS bulk-send create validation, lookup/documents, and webhook CRUD");
    await Task.Delay(TimeSpan.FromSeconds(60));

    using (var completedPdf = await client.Documents.GetCompletedPdfStreamAsync(completedDocumentId))
    {
        require(completedPdf.ReadByte() >= 0, "Completed PDF stream is empty.");
        await completedPdf.CopyToAsync(Stream.Null);
    }
    require(!string.IsNullOrWhiteSpace((await client.Documents.GetCompletedPdfUrlAsync(completedDocumentId)).FileUrl),
        "Completed PDF URL is missing.");
    Console.WriteLine("PASS completed PDF stream and URL using supplied fixture");

    if (nom151DocumentId is { } certificateDocumentId)
    {
        using (var certificateZip = await client.Regional.GetNom151StreamAsync(certificateDocumentId))
        {
            require(certificateZip.ReadByte() >= 0, "NOM-151 stream is empty.");
            await certificateZip.CopyToAsync(Stream.Null);
        }
        require(!string.IsNullOrWhiteSpace((await client.Regional.GetNom151UrlAsync(certificateDocumentId)).FileUrl),
            "NOM-151 URL is missing.");
        require(!string.IsNullOrWhiteSpace((await client.Regional.GetNom151ObjectAsync(certificateDocumentId)).Nom151?.Status),
            "NOM-151 certificate status is missing.");
        Console.WriteLine("PASS NOM-151 stream, URL, and object using supplied fixture");
    }
    else
    {
        Console.WriteLine("SKIP NOM-151 success modes: SIGNWELL_NOM151_DOCUMENT_ID is not configured.");
    }

    await ExpectNotFound(() => client.Documents.GetCompletedPdfStreamAsync(Guid.NewGuid()), "completed PDF stream");
    await ExpectNotFound(() => client.Documents.GetCompletedPdfUrlAsync(Guid.NewGuid()), "completed PDF URL");
    await ExpectNotFound(() => client.Regional.GetNom151StreamAsync(Guid.NewGuid()), "NOM-151 stream");
    await ExpectNotFound(() => client.Regional.GetNom151UrlAsync(Guid.NewGuid()), "NOM-151 URL");
    await ExpectNotFound(() => client.Regional.GetNom151ObjectAsync(Guid.NewGuid()), "NOM-151 object");
    await ExpectNotFound(() => client.ApiApplications.GetAsync(Guid.NewGuid()), "API application lookup");
    await ExpectNotFound(() => client.ApiApplications.DeleteAsync(Guid.NewGuid()), "API application delete");
    await client.Templates.DeleteAsync(secondTemplate);
    templates.Remove(secondTemplate);
    Console.WriteLine("PASS typed failures and template delete");
}
finally
{
    foreach (var id in documents.AsEnumerable().Reverse())
    {
        try { await client.Documents.DeleteAsync(id); }
        catch (SignWellException exception) { Console.Error.WriteLine($"Cleanup failed for document {id}: {exception.GetType().Name}"); }
    }
    foreach (var id in templates.AsEnumerable().Reverse())
    {
        try { await client.Templates.DeleteAsync(id); }
        catch (SignWellException exception) { Console.Error.WriteLine($"Cleanup failed for template {id}: {exception.GetType().Name}"); }
    }
}

static async Task<Guid> CreateTemplateAsync(SignWellClient client, List<Guid> created, string? fileUrl, byte[]? fileBase64, string suffix)
{
    var file = fileUrl is not null
        ? new FilesInner("fixture.pdf", fileUrl: fileUrl)
        : new FilesInner("fixture.pdf", fileBase64: fileBase64!);
    var response = await client.CreateTemplateAsync(new DocumentTemplateRequest(
        [file],
        [new PlaceholdersInner("1", "Signer")],
        name: $".NET SDK live acceptance template {suffix}",
        fields: new List<List<TemplateFieldsInnerInner>>
        {
            new() { new TemplateFieldsInnerInner(72, 520, 1, "1", FieldType.Signature) }
        }));
    created.Add(response.Id);
    return response.Id;
}

static async Task<DocumentResponse> WaitForDraftAsync(SignWellClient client, Guid id)
{
    for (var attempt = 0; attempt < 15; attempt++)
    {
        var response = await client.Documents.GetAsync(id);
        if (string.Equals(response.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return response;
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
    throw new InvalidOperationException("Document did not become Draft before send.");
}

static async Task<DocumentResponse> WaitForSentAsync(SignWellClient client, Guid id)
{
    for (var attempt = 0; attempt < 15; attempt++)
    {
        var response = await client.Documents.GetAsync(id);
        if (response.Status is "Sent" or "Pending" or "Viewed" or "Bounced")
            return response;
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
    throw new InvalidOperationException("Document did not become sent before recipient update.");
}

static void ErrorCheck<T>(T response) where T : SignWell.Sdk.Client.IApiResponse
{
    if (!response.IsSuccessStatusCode)
        throw new SignWellException($"Unexpected HTTP {(int)response.StatusCode} from live acceptance request.");
}

static void require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
