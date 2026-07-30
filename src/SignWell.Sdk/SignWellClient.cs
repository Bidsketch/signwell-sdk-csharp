using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignWell.Sdk.Client;
using SignWell.Sdk.Embedded;
using SignWell.Sdk.Extensions;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;
using SignWell.Sdk.Resources;

namespace SignWell.Sdk;

public interface ISignWellClient
{
    DocumentsResource Documents { get; }
    TemplatesResource Templates { get; }
    BulkSendsResource BulkSends { get; }
    RegionalResource Regional { get; }
    MeResource Me { get; }
    ApiApplicationsResource ApiApplications { get; }
    WebhooksResource Webhooks { get; }
    EmbeddedWorkflows Embedded { get; }

    Task<DocumentResponse> CreateDocumentAsync(DocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentFromTemplateResponse> CreateDocumentFromTemplateAsync(DocumentFromTemplateRequest request, CancellationToken cancellationToken = default);
    Task<DocumentResponse> SendDocumentAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default);
    Task<DocumentResponse> UpdateDocumentAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default);
    Task SendReminderAsync(Guid id, SendReminderRequest request, CancellationToken cancellationToken = default);
    Task<MeResponse> GetMeAsync(CancellationToken cancellationToken = default);
    Task<DocumentTemplateResponse> CreateTemplateAsync(DocumentTemplateRequest request, CancellationToken cancellationToken = default);
    Task<DocumentTemplateResponse> UpdateTemplateAsync(Guid id, DocumentTemplateUpdateRequest request, CancellationToken cancellationToken = default);
    Task<BulkSendCreateResponse> CreateBulkSendAsync(CreateBulkSendRequest request, CancellationToken cancellationToken = default);
    Task<BulkSendValidateCsvResponse> ValidateBulkSendCsvAsync(BulkSendCsvRequest request, CancellationToken cancellationToken = default);
    Task<OwnedResponseStream> GetCompletedPdfStreamAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompletedPdfUrlResponse> GetCompletedPdfUrlAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class SignWellClient : ISignWellClient, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly bool _ownsProvider;

    public SignWellClient(string apiKey)
        : this(new SignWellClientOptions { ApiKey = apiKey }) { }

    public SignWellClient(string apiKey, Uri baseUri)
        : this(new SignWellClientOptions { ApiKey = apiKey, BaseUri = baseUri }) { }

    public SignWellClient(SignWellClientOptions options)
    {
        var services = new ServiceCollection();
        SignWellServiceCollectionExtensions.AddSignWellCore(services, options);
        _services = services.BuildServiceProvider();
        _ownsProvider = true;
        Initialize();
    }

    internal SignWellClient(IServiceProvider services)
    {
        _services = services;
        _ownsProvider = false;
        Initialize();
    }

    public DocumentsResource Documents { get; private set; } = null!;
    public TemplatesResource Templates { get; private set; } = null!;
    public BulkSendsResource BulkSends { get; private set; } = null!;
    public RegionalResource Regional { get; private set; } = null!;
    public MeResource Me { get; private set; } = null!;
    public ApiApplicationsResource ApiApplications { get; private set; } = null!;
    public WebhooksResource Webhooks { get; private set; } = null!;
    public EmbeddedWorkflows Embedded { get; private set; } = null!;

    private void Initialize()
    {
        Documents = new DocumentsResource(_services.GetRequiredService<IDocumentApi>());
        Templates = new TemplatesResource(_services.GetRequiredService<ITemplateApi>());
        BulkSends = new BulkSendsResource(_services.GetRequiredService<IBulkSendApi>(), _services.GetRequiredService<ResponseModesClient>());
        Regional = new RegionalResource(_services.GetRequiredService<IRegionalApi>(), _services.GetRequiredService<ResponseModesClient>());
        Me = new MeResource(_services.GetRequiredService<IMeApi>());
        ApiApplications = new ApiApplicationsResource(_services.GetRequiredService<IApiApplicationApi>());
        Webhooks = new WebhooksResource(_services.GetRequiredService<IWebhooksApi>());
        Documents.ResponseModes = _services.GetRequiredService<ResponseModesClient>();
        Embedded = new EmbeddedWorkflows(Documents);
    }

    public Task<DocumentResponse> CreateDocumentAsync(DocumentRequest request, CancellationToken cancellationToken = default) =>
        Documents.CreateAsync(request, cancellationToken);

    public Task<DocumentFromTemplateResponse> CreateDocumentFromTemplateAsync(DocumentFromTemplateRequest request, CancellationToken cancellationToken = default) =>
        Documents.CreateFromTemplateAsync(request, cancellationToken);

    public Task<DocumentResponse> SendDocumentAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default) =>
        Documents.SendAsync(id, request, cancellationToken);

    public Task<DocumentResponse> UpdateDocumentAsync(Guid id, UpdateDocumentAndSendRequest request, CancellationToken cancellationToken = default) =>
        Documents.UpdateDocumentAsync(id, request, cancellationToken);

    public Task SendReminderAsync(Guid id, SendReminderRequest request, CancellationToken cancellationToken = default) =>
        Documents.SendReminderAsync(id, request, cancellationToken);

    public Task<MeResponse> GetMeAsync(CancellationToken cancellationToken = default) =>
        Me.GetAsync(cancellationToken);

    public Task<DocumentTemplateResponse> CreateTemplateAsync(DocumentTemplateRequest request, CancellationToken cancellationToken = default) =>
        Templates.CreateAsync(request, cancellationToken);

    public Task<DocumentTemplateResponse> UpdateTemplateAsync(Guid id, DocumentTemplateUpdateRequest request, CancellationToken cancellationToken = default) =>
        Templates.UpdateAsync(id, request, cancellationToken);

    public Task<BulkSendCreateResponse> CreateBulkSendAsync(CreateBulkSendRequest request, CancellationToken cancellationToken = default) =>
        BulkSends.CreateAsync(request, cancellationToken);

    public Task<BulkSendValidateCsvResponse> ValidateBulkSendCsvAsync(BulkSendCsvRequest request, CancellationToken cancellationToken = default) =>
        BulkSends.ValidateCsvAsync(request, cancellationToken);

    public Task<OwnedResponseStream> GetCompletedPdfStreamAsync(Guid id, CancellationToken cancellationToken = default) =>
        Documents.GetCompletedPdfStreamAsync(id, cancellationToken: cancellationToken);

    public Task<CompletedPdfUrlResponse> GetCompletedPdfUrlAsync(Guid id, CancellationToken cancellationToken = default) =>
        Documents.GetCompletedPdfUrlAsync(id, cancellationToken: cancellationToken);

    public void Dispose()
    {
        if (_ownsProvider && _services is IDisposable disposable)
            disposable.Dispose();
    }
}

public static class SignWellServiceCollectionExtensions
{
    public static IServiceCollection AddSignWell(
        this IServiceCollection services,
        Action<SignWellClientOptions> configure)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var options = new SignWellClientOptions();
        configure(options);
        AddSignWellCore(services, options);
        services.AddSingleton<SignWellClient>();
        services.AddSingleton<ISignWellClient>(sp => sp.GetRequiredService<SignWellClient>());
        return services;
    }

    public static IServiceCollection AddSignWell(this IServiceCollection services, SignWellClientOptions options)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        AddSignWellCore(services, options);
        services.AddSingleton<SignWellClient>();
        services.AddSingleton<ISignWellClient>(sp => sp.GetRequiredService<SignWellClient>());
        return services;
    }

    internal static void AddSignWellCore(IServiceCollection services, SignWellClientOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        options.Validate();
        services.AddSingleton(options);
        services.AddLogging(builder => builder.AddFilter("SignWell.Sdk.Raw", LogLevel.None));
        services.AddApi(host => host
            .AddApiHttpClients(
                client =>
                {
                    client.BaseAddress = options.BaseUri;
                    client.Timeout = options.Timeout;
                    client.DefaultRequestHeaders.Remove("X-Api-Key");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SignWell C# SDK/0.1.0-beta.1");
                },
                options.ConfigureHttpClientBuilder)
            .AddTokens(new ApiKeyToken(options.ApiKey, ClientUtils.ApiKeyHeader.X_Api_Key, string.Empty)));

        var streaming = services.AddHttpClient(
            ResponseModesClient.HttpClientName,
            client =>
            {
                client.BaseAddress = options.BaseUri;
                client.Timeout = options.Timeout;
                client.DefaultRequestHeaders.Remove("X-Api-Key");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SignWell C# SDK/0.1.0-beta.1");
            });
        options.ConfigureHttpClientBuilder?.Invoke(streaming);
        services.AddSingleton<ResponseModesClient>();
    }
}
