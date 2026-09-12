namespace DarkStrata.CredentialCheck.Umbraco;

/// <summary>
/// Package settings, bound from the <c>DarkStrata:CredentialCheck</c> configuration section.
/// </summary>
public sealed class DarkStrataOptions
{
    public const string SectionName = "DarkStrata:CredentialCheck";

    /// <summary>HTTP header carrying the shared secret on inbound DarkStrata webhooks.</summary>
    public const string WebhookSecretHeader = "X-DarkStrata-Secret";

    /// <summary>ASP.NET Identity error code returned when a password is rejected.</summary>
    public const string IdentityErrorCode = "DarkStrataCompromised";

    /// <summary>DarkStrata API key with the <c>credential_check:read</c> scope.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Override the API base URL (defaults to the SDK default).</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Reject compromised passwords when members or backoffice users set or change them.</summary>
    public bool ValidatePasswords { get; set; } = true;

    /// <summary>Check backoffice logins against the breach corpus.</summary>
    public bool CheckBackOfficeLogin { get; set; } = true;

    /// <summary>What to do when a backoffice login uses a compromised password.</summary>
    public BackOfficeLoginAction BackOfficeLoginAction { get; set; } = BackOfficeLoginAction.Deny;

    /// <summary>Shared secret expected in <see cref="WebhookSecretHeader"/>. Webhook endpoint is disabled when empty.</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>When the DarkStrata API is unreachable, allow the operation (true) or reject it (false).</summary>
    public bool FailOpen { get; set; } = true;
}

public enum BackOfficeLoginAction
{
    /// <summary>Reject the login as invalid credentials.</summary>
    Deny,

    /// <summary>Allow the login but log a warning and publish <see cref="CompromisedCredentialDetectedNotification"/>.</summary>
    Warn,
}
