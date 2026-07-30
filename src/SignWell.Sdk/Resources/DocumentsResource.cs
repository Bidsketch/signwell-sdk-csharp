using System.Runtime.CompilerServices;
using SignWell.Sdk.Client;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;

namespace SignWell.Sdk.Resources;

public sealed class DocumentsResource
{
    private static readonly HashSet<string> DefaultTerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Completed", "Manually completed", "Declined", "Canceled",
        "Bounced", "Blocked", "Error", "Expired"
    };

    internal DocumentsResource(IDocumentApi raw) => Raw = raw;

    public IDocumentApi Raw { get; }
    internal ResponseModesClient ResponseModes { get; set; } = null!;

    public Task<DocumentResponse> CreateAsync(DocumentRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.CreateDocumentAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty document response.");
        }, cancellationToken);

    public Task<DocumentFromTemplateResponse> CreateFromTemplateAsync(DocumentFromTemplateRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.CreateDocumentFromTemplateAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty document response.");
        }, cancellationToken);

    public Task<DocumentResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.GetDocumentAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty document response.");
        }, cancellationToken);

    public Task<DocumentResponse> SendAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.SendDocumentAsync(id, request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty document response.");
        }, cancellationToken);

    public Task<DocumentResponse> UpdateDocumentAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(id, request, cancellationToken);

    public Task SendReminderAsync(Guid id, SendReminderRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.SendReminderAsync(id, request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return true;
        }, cancellationToken);

    public Task<DocumentResponse> UpdateRecipientsAsync(Guid id, UpdateRecipientsRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.UpdateRecipientsAsync(id, request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty document response.");
        }, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.DeleteDocumentAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return true;
        }, cancellationToken);

    public async IAsyncEnumerable<DocumentListResponse> GetPagesAsync(
        string? query = null,
        int page = 1,
        int limit = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ValidateListArguments(query, page, ref limit);
        while (true)
        {
            var response = await SdkCall.ExecuteAsync(
                () => Raw.ListDocumentsAsync(page, limit, ToOption(query), cancellationToken),
                cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            var result = response.Ok() ?? throw new SignWellException("SignWell returned an empty document list.");
            yield return result;
            if (result.NextPage is null)
                yield break;
            page = result.NextPage.Value;
        }
    }

    public async IAsyncEnumerable<DocumentResponse> GetItemsAsync(
        string? query = null,
        int page = 1,
        int limit = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in GetPagesAsync(query, page, limit, cancellationToken).ConfigureAwait(false))
            foreach (var document in response.Documents)
                yield return document;
    }

    public async Task<DocumentResponse> WaitForCompletionAsync(
        Guid id,
        TimeSpan? pollingInterval = null,
        TimeSpan? timeout = null,
        int? maximumAttempts = null,
        IEnumerable<string>? terminalStatuses = null,
        CancellationToken cancellationToken = default)
    {
        var interval = pollingInterval ?? TimeSpan.FromSeconds(2);
        var duration = timeout ?? TimeSpan.FromMinutes(2);
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollingInterval));
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        if (maximumAttempts is <= 0) throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        var statuses = terminalStatuses is null
            ? DefaultTerminalStatuses
            : new HashSet<string>(terminalStatuses, StringComparer.OrdinalIgnoreCase);
        if (statuses.Count == 0) throw new ArgumentException("At least one terminal status is required.", nameof(terminalStatuses));

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(duration);
        DocumentResponse? last = null;
        var attempt = 0;
        try
        {
            while (maximumAttempts is null || attempt < maximumAttempts.Value)
            {
                attempt++;
                last = await GetAsync(id, deadline.Token).ConfigureAwait(false);
                if (last.Status is not null && statuses.Contains(last.Status))
                    return last;
                if (maximumAttempts is not null && attempt >= maximumAttempts.Value)
                    break;
                await Task.Delay(interval, deadline.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && deadline.IsCancellationRequested)
        {
            throw new WaitForCompletionTimeoutException(last);
        }
        throw new WaitForCompletionTimeoutException(last);
    }

    public Task<OwnedResponseStream> GetCompletedPdfStreamAsync(
        Guid id,
        bool auditPage = false,
        FileFormat? fileFormat = null,
        CancellationToken cancellationToken = default) =>
        ResponseModes.GetCompletedPdfStreamAsync(id, auditPage, fileFormat, cancellationToken);

    public Task<CompletedPdfUrlResponse> GetCompletedPdfUrlAsync(
        Guid id,
        bool auditPage = false,
        FileFormat? fileFormat = null,
        CancellationToken cancellationToken = default) =>
        ResponseModes.GetCompletedPdfUrlAsync(id, auditPage, fileFormat, cancellationToken);

    internal static void ValidateListArguments(string? query, int page, ref int limit)
    {
        if (query is not null && string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be blank.", nameof(query));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be at least 1.");
        limit = Math.Min(limit, 50);
    }

    private static Option<string> ToOption(string? value) =>
        value is null ? default : new Option<string>(value);
}
