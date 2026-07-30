using SignWell.Sdk.Errors;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;

namespace SignWell.Sdk.Resources;

public sealed class MeResource
{
    internal MeResource(IMeApi raw) => Raw = raw;
    public IMeApi Raw { get; }
    public Task<MeResponse> GetAsync(CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.GetMeAsync(cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty account response.");
        }, cancellationToken);
}

public sealed class ApiApplicationsResource
{
    internal ApiApplicationsResource(IApiApplicationApi raw) => Raw = raw;
    public IApiApplicationApi Raw { get; }
    public Task<ApiApplicationResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.GetApiApplicationAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty API application response.");
        }, cancellationToken);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.DeleteApiApplicationAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return true;
        }, cancellationToken);
}

public sealed class WebhooksResource
{
    internal WebhooksResource(IWebhooksApi raw) => Raw = raw;
    public IWebhooksApi Raw { get; }
    public Task<WebhookResponse> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.CreateWebhookAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty webhook response.");
        }, cancellationToken);
    public Task<IReadOnlyList<WebhookResponse>> ListAsync(CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.ListWebhooksAsync(cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return (IReadOnlyList<WebhookResponse>)(response.Ok() ?? new List<WebhookResponse>());
        }, cancellationToken);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.DeleteWebhookAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return true;
        }, cancellationToken);
}

public sealed class BulkSendsResource
{
    private readonly ResponseModesClient _responseModes;
    internal BulkSendsResource(IBulkSendApi raw, ResponseModesClient responseModes)
    {
        Raw = raw;
        _responseModes = responseModes;
    }
    public IBulkSendApi Raw { get; }
    public Task<BulkSendCreateResponse> CreateAsync(CreateBulkSendRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.CreateBulkSendAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Created() ?? throw new SignWellException("SignWell returned an empty bulk-send response.");
        }, cancellationToken);
    public Task<BulkSendResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.GetBulkSendAsync(id, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty bulk-send response.");
        }, cancellationToken);
    public Task<BulkSendValidateCsvResponse> ValidateCsvAsync(BulkSendCsvRequest request, CancellationToken cancellationToken = default) =>
        SdkCall.ExecuteAsync(async () =>
        {
            var response = await Raw.ValidateBulkSendCsvAsync(request, cancellationToken).ConfigureAwait(false);
            ErrorMapper.ThrowIfError(response);
            return response.Ok() ?? throw new SignWellException("SignWell returned an empty bulk-send validation response.");
        }, cancellationToken);
    public Task<OwnedResponseStream> GetCsvTemplateStreamAsync(IReadOnlyCollection<Guid> templateIds, CancellationToken cancellationToken = default) =>
        _responseModes.GetBulkSendCsvStreamAsync(templateIds, cancellationToken);
    public Task<BulkSendCsvTemplateResponse> GetCsvTemplateBase64Async(IReadOnlyCollection<Guid> templateIds, CancellationToken cancellationToken = default) =>
        _responseModes.GetBulkSendCsvBase64Async(templateIds, cancellationToken);
}

public sealed class RegionalResource
{
    private readonly ResponseModesClient _responseModes;
    internal RegionalResource(IRegionalApi raw, ResponseModesClient responseModes)
    {
        Raw = raw;
        _responseModes = responseModes;
    }
    public IRegionalApi Raw { get; }
    public Task<OwnedResponseStream> GetNom151StreamAsync(Guid id, CancellationToken cancellationToken = default) =>
        _responseModes.GetNom151StreamAsync(id, cancellationToken);
    public Task<Nom151UrlResponse> GetNom151UrlAsync(Guid id, CancellationToken cancellationToken = default) =>
        _responseModes.GetNom151UrlAsync(id, cancellationToken);
    public Task<Nom151CertificateResponse> GetNom151ObjectAsync(Guid id, CancellationToken cancellationToken = default) =>
        _responseModes.GetNom151ObjectAsync(id, cancellationToken);
}
