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

### 1. Create an API key

In [app.darkstrata.io](https://app.darkstrata.io) go to **Integrations → API keys → Create key**
and tick the **`credential_check:read`** scope. Copy the key (it starts with `ds_live_`); it is
shown once. The scope is only offered on plans that include Credential Check, so if you cannot
select it, your plan needs upgrading first.

### 2. Give the key to the site

The package reads the standard ASP.NET Core configuration section `DarkStrata:CredentialCheck`,
so use whichever configuration source your hosting already uses. Pick **one**:

**Environment variable** (recommended for production, Umbraco Cloud, Azure App Service, Docker):

```
DarkStrata__CredentialCheck__ApiKey=ds_live_...
```

**User secrets** (local development, keeps the key out of git):

```bash
dotnet user-secrets set "DarkStrata:CredentialCheck:ApiKey" "ds_live_..."
```

**`appsettings.json`** (simplest, but the key is then committed with your site):

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

Azure Key Vault, AWS Parameter Store and similar work through their normal configuration
providers; nothing package-specific is needed. Changes are picked up without a restart where
the provider supports reload.

### 3. Check it is working

Open **Settings → Health Check → Security** in the backoffice. **DarkStrata Credential Check**
is green when the key is configured and accepted by the API. It is red with a message when the
key is missing or rejected.

There is no backoffice settings screen for the key. It is a secret, so it lives in configuration,
not in the CMS database.

### What happens if the key is missing or wrong

| State | Behaviour |
|---|---|
| No key configured | One warning is logged at startup. Every check is skipped; logins and password changes behave as if the package were not installed. |
| Key rejected (401) or API unreachable | With `FailOpen: true` (default) the operation is allowed and a warning is logged. With `FailOpen: false` the login or password change is refused. |

### All settings

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
