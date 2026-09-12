using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;

namespace DarkStrata.CredentialCheck.Umbraco;

/// <summary>
/// Receives DarkStrata webhook deliveries at <c>/umbraco/darkstrata/webhook</c> and republishes them
/// as <see cref="DarkStrataAlertReceivedNotification"/>. Authenticated by a shared secret header
/// because DarkStrata webhooks are not signed.
/// </summary>
[AllowAnonymous]
[Route(RoutePath)]
public sealed class DarkStrataWebhookController : ControllerBase
{
    public const string RoutePath = "umbraco/darkstrata/webhook";
    private const string EventProperty = "event";

    private readonly IEventAggregator _events;
    private readonly IOptionsMonitor<DarkStrataOptions> _options;

    public DarkStrataWebhookController(IEventAggregator events, IOptionsMonitor<DarkStrataOptions> options)
    {
        _events = events;
        _options = options;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        var secret = _options.CurrentValue.WebhookSecret;
        if (string.IsNullOrEmpty(secret))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var presented = Request.Headers[DarkStrataOptions.WebhookSecretHeader].ToString();
        if (!FixedTimeEquals(presented, secret))
        {
            return Unauthorized();
        }

        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return BadRequest();
        }

        using (document)
        {
            var payload = document.RootElement.Clone();
            var eventKey = payload.ValueKind == JsonValueKind.Object
                && payload.TryGetProperty(EventProperty, out var eventElement)
                && eventElement.ValueKind == JsonValueKind.String
                ? eventElement.GetString()!
                : string.Empty;

            await _events.PublishAsync(new DarkStrataAlertReceivedNotification(eventKey, payload), cancellationToken);
        }

        return Accepted();
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
