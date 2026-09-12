using System.Text.Json;
using Umbraco.Cms.Core.Notifications;

namespace DarkStrata.CredentialCheck.Umbraco;

public enum CompromisedCredentialSource
{
    MemberPassword,
    BackOfficePassword,
    BackOfficeLogin,
    FormsWorkflow,
}

/// <summary>
/// Published whenever a checked email/password pair is found in the DarkStrata breach corpus.
/// Handle it with an INotificationAsyncHandler to trigger your own workflow.
/// </summary>
public sealed class CompromisedCredentialDetectedNotification : INotification
{
    public CompromisedCredentialDetectedNotification(CompromisedCredentialSource source, string email, string? userId)
    {
        Source = source;
        Email = email;
        UserId = userId;
    }

    public CompromisedCredentialSource Source { get; }

    public string Email { get; }

    /// <summary>Identity user id (member or backoffice user) when known.</summary>
    public string? UserId { get; }
}

/// <summary>
/// Published for every authenticated DarkStrata webhook delivery.
/// </summary>
public sealed class DarkStrataAlertReceivedNotification : INotification
{
    public DarkStrataAlertReceivedNotification(string eventKey, JsonElement payload)
    {
        EventKey = eventKey;
        Payload = payload;
    }

    /// <summary>DarkStrata event key, e.g. <c>alert.stealer_log.created</c>.</summary>
    public string EventKey { get; }

    /// <summary>Full webhook body: <c>{ event, data, timestamp, webhook_id }</c>.</summary>
    public JsonElement Payload { get; }
}
