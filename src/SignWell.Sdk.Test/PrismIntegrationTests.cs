using Microsoft.Extensions.DependencyInjection;
using SignWell.Sdk.Client;
using SignWell.Sdk.Extensions;
using SignWell.Sdk.Models;
using SignWell.Sdk.Raw;
using Xunit;

namespace SignWell.Sdk.Test;

public sealed class PrismIntegrationTests
{
    [Fact]
    [Trait("Category", "Prism")]
    public async Task GeneratedClientsSerializeRequestsAndParseResponses()
    {
        var prismUrl = Environment.GetEnvironmentVariable("PRISM_URL");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(prismUrl), "PRISM_URL is not configured.");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApi(configuration => configuration
            .AddApiHttpClients(client => client.BaseAddress = new Uri(prismUrl!))
            .AddTokens(new ApiKeyToken("prism-test", ClientUtils.ApiKeyHeader.X_Api_Key, string.Empty)));
        using var provider = services.BuildServiceProvider();

        var me = await provider.GetRequiredService<IMeApi>().GetMeAsync(TestContext.Current.CancellationToken);
        Assert.True(me.IsOk);
        Assert.NotNull(me.Ok());

        var webhook = await provider.GetRequiredService<IWebhooksApi>().CreateWebhookAsync(
            new CreateWebhookRequest("https://example.com/signwell-webhook"),
            TestContext.Current.CancellationToken);
        Assert.True(webhook.IsCreated);
        Assert.NotNull(webhook.Created());
    }
}
