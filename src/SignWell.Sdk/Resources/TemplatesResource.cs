using System.Runtime.CompilerServices;
using SignWell.Sdk.Client;
using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;

namespace SignWell.Sdk.Resources;

public sealed class TemplatesResource
{
    internal TemplatesResource(ITemplateApi raw) => Raw = raw;
    public ITemplateApi Raw { get; }

    public Task<DocumentTemplateResponse> CreateAsync(DocumentTemplateRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.CreateTemplateAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty template response.");
        }, cancellationToken);

    public Task<DocumentTemplateResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.GetTemplateAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty template response.");
        }, cancellationToken);

    public Task<DocumentTemplateResponse> UpdateAsync(Guid id, DocumentTemplateUpdateRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.UpdateTemplateAsync(id, request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty template response.");
        }, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.DeleteTemplateAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return true;
        }, cancellationToken);

    public async IAsyncEnumerable<DocumentTemplateListResponse> GetPagesAsync(
        string? query = null,
        int page = 1,
        int limit = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DocumentsResource.ValidateListArguments(query, page, ref limit);
        while (true)
        {
            Option<string> queryOption = query is null ? default : new Option<string>(query);
            var response = await SdkCall.ExecuteAsync(
                () => Raw.ListTemplatesAsync(page, limit, queryOption, cancellationToken),
                cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            var result = response.Ok() ?? throw new SignWellException("SignWell returned an empty template list.");
            yield return result;
            if (result.NextPage is null)
                yield break;
            page = result.NextPage.Value;
        }
    }

    public async IAsyncEnumerable<DocumentTemplateResponse> GetItemsAsync(
        string? query = null,
        int page = 1,
        int limit = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in GetPagesAsync(query, page, limit, cancellationToken).ConfigureAwait(false))
            foreach (var template in response.Templates)
                yield return template;
    }
}
