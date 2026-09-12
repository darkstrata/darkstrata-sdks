# DarkStrata Credential Check for Umbraco

Stops compromised credentials being used on your Umbraco site. Every check uses
[k-anonymity](https://github.com/darkstrata/darkstrata-sdks#how-k-anonymity-works):
only the first 5 characters of a SHA-256 hash ever leave your server. Passwords
are never sent to DarkStrata.

Supports Umbraco 13 LTS, 15 and 16. Server-side only, no backoffice UI.

## What it does

| Hook | Behaviour |
|---|---|
| Password set / change / reset (members and backoffice users) | Rejected with identity error code `DarkStrataCompromised` when the email + password pair is in the breach corpus. |
| Backoffice login | Compromised pair is denied (default) or allowed with a warning. |
| Any hit | Publishes `CompromisedCredentialDetectedNotification` so you can run your own workflow. |
| DarkStrata alert webhooks | `POST /umbraco/darkstrata/webhook` republishes each delivery as `DarkStrataAlertReceivedNotification`. |
| Health check | Settings → Health Check → Security shows API key and connectivity status. |

## Install

```bash
dotnet add package DarkStrata.CredentialCheck.Umbraco
```

## Configure

Create an API key with the `credential_check:read` scope at
[app.darkstrata.io](https://app.darkstrata.io), then add to `appsettings.json`:

```json
{
  "DarkStrata": {
    "CredentialCheck": {
      "ApiKey": "ds_live_...",
      "ValidatePasswords": true,
      "CheckBackOfficeLogin": true,
      "BackOfficeLoginAction": "Deny",
      "WebhookSecret": "a-long-random-string",
      "FailOpen": true
    }
  }
}
```

| Setting | Default | Meaning |
|---|---|---|
| `ApiKey` | — | Required. With no key every check is skipped and a warning is logged at startup. |
| `ValidatePasswords` | `true` | Reject compromised passwords when they are set or changed. |
| `CheckBackOfficeLogin` | `true` | Check backoffice logins. |
| `BackOfficeLoginAction` | `Deny` | `Deny` rejects the login. `Warn` allows it, logs a warning and publishes the notification. |
| `WebhookSecret` | — | Shared secret for the webhook endpoint. Endpoint returns 503 until set. |
| `FailOpen` | `true` | If the DarkStrata API is unreachable, allow the operation. Set `false` to reject instead. |

## React to a hit (your workflow)

```csharp
using DarkStrata.CredentialCheck.Umbraco;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;

public class CompromisedCredentialHandler : INotificationAsyncHandler<CompromisedCredentialDetectedNotification>
{
    public Task HandleAsync(CompromisedCredentialDetectedNotification n, CancellationToken ct)
    {
        // n.Source: MemberPassword | BackOfficePassword | BackOfficeLogin
        // n.Email, n.UserId
        // e.g. email the security team, lock the member, open a ticket
        return Task.CompletedTask;
    }
}

public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) =>
        builder.AddNotificationAsyncHandler<CompromisedCredentialDetectedNotification, CompromisedCredentialHandler>();
}
```

## Receive DarkStrata alerts

1. In DarkStrata go to Integrations → Webhooks → Add.
2. URL: `https://your-site.example/umbraco/darkstrata/webhook`
3. Authentication: **API Key**, header name `X-DarkStrata-Secret`, value = your `WebhookSecret`.
4. Subscribe to the events you care about, e.g. `alert.stealer_log.created`, `alert.breach_exposure.created`.

Each delivery is published as `DarkStrataAlertReceivedNotification` with `EventKey` and the
raw JSON `Payload` (`{ event, data, timestamp, webhook_id }`). Handle it the same way as above.

## Support

- Issues: https://github.com/darkstrata/darkstrata-sdks/issues
- Docs: https://docs.darkstrata.io
