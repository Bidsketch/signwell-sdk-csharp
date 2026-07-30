# SignWell C# SDK

The official, nullable, async-first C# client for the SignWell API.

```csharp
using SignWell.Sdk;

await using var client = new SignWellClient("YOUR_API_KEY");
var account = await client.Me.GetAsync();
```

For dependency injection:

```csharp
services.AddSignWell(options => options.ApiKey = configuration["SignWell:ApiKey"]!);
```

The package targets `netstandard2.0` and `net10.0`. Generated response-level
clients are exposed under `SignWell.Sdk.Raw`; the `SignWellClient` facade adds
typed exceptions, pagination, polling, safe streaming downloads, embedded
renderers, and webhook verification.

Automatic retries are intentionally disabled. Configure resilience through
`SignWellClientOptions.ConfigureHttpClientBuilder` after considering endpoint
idempotency.

Replay protection for webhooks requires a shared atomic database or Redis
implementation of `IWebhookReplayStore` in multi-instance production systems.
The included in-memory store is intended for tests and single-process services.
