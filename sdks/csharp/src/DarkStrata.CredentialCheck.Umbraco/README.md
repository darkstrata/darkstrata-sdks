# DarkStrata Credential Check for Umbraco

Stops compromised credentials being used on your Umbraco site. When a member or
backoffice user logs in or sets a password, the email + password pair is hashed
locally and checked against the DarkStrata breach corpus using
[k-anonymity](https://github.com/darkstrata/darkstrata-sdks#how-k-anonymity-works):
only the first 5 characters of a SHA-256 hash ever leave your server. Passwords
are never sent to DarkStrata. That is the package's only interaction with the API.

Supports Umbraco 13 LTS, 15 and 16. Server-side only, no backoffice UI.

## What it does

| Hook | Behaviour |
|---|---|
| Password set / change / reset (members and backoffice users) | Rejected with identity error code `DarkStrataCompromised` when the email + password pair is in the breach corpus. |
| Member and backoffice login | Compromised pair is denied as a wrong password, counting towards Umbraco's lockout threshold (default), or allowed with a warning. |
| Any hit | Publishes `CompromisedCredentialDetectedNotification` so you can run your own workflow. |
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
      "CheckLogins": true,
      "LoginAction": "Deny",
      "FailOpen": true
    }
  }
}
```

| Setting | Default | Meaning |
|---|---|---|
| `ApiKey` | — | Required. With no key every check is skipped and a warning is logged at startup. |
| `ValidatePasswords` | `true` | Reject compromised passwords when they are set or changed. |
| `CheckLogins` | `true` | Check member and backoffice logins. |
| `LoginAction` | `Deny` | `Deny` rejects the login like a wrong password (lockout rules apply). `Warn` allows it, logs a warning and publishes the notification. |
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
        // n.Source: MemberPassword | BackOfficePassword | MemberLogin | BackOfficeLogin
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

## Support

- Issues: https://github.com/darkstrata/darkstrata-sdks/issues
- Docs: https://docs.darkstrata.io
