using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace DarkStrata.CredentialCheck.Umbraco;

/// <summary>
/// Single gate every hook goes through: config guard, SDK call, fail-open handling, notification.
/// </summary>
public sealed class CompromisedCredentialService
{
    private readonly IDarkStrataCredentialCheck _client;
    private readonly IEventAggregator _events;
    private readonly IOptionsMonitor<DarkStrataOptions> _options;
    private readonly ILogger<CompromisedCredentialService> _logger;
    private readonly IRequestCache _requestCache;

    public CompromisedCredentialService(
        IDarkStrataCredentialCheck client,
        IEventAggregator events,
        IOptionsMonitor<DarkStrataOptions> options,
        ILogger<CompromisedCredentialService> logger,
        IRequestCache requestCache)
    {
        _client = client;
        _events = events;
        _options = options;
        _logger = logger;
        _requestCache = requestCache;
    }

    /// <summary>
    /// Returns true when the pair is in the breach corpus. Returns false when the check is
    /// unconfigured or fails and <see cref="DarkStrataOptions.FailOpen"/> is set; throws otherwise.
    /// The result is memoised for the rest of the request, so repeated checks of the same
    /// credentials cost one API call and raise one notification.
    /// </summary>
    public async Task<bool> IsCompromisedAsync(
        CompromisedCredentialSource source,
        string? email,
        string? password,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        // Umbraco checks the same credentials more than once per login: AuthenticationController
        // calls PasswordSignInAsync and then CheckPasswordAsync again to tell a wrong password
        // apart from a lockout. Without this the key owner pays for every duplicate.
        var cached = _requestCache.Get(
            CacheKey(source, email, password),
            () => CheckAsync(source, email, password, userId, options, cancellationToken));

        return cached is Task<bool> task
            ? await task
            : await CheckAsync(source, email, password, userId, options, cancellationToken);
    }

    private async Task<bool> CheckAsync(
        CompromisedCredentialSource source,
        string email,
        string password,
        string? userId,
        DarkStrataOptions options,
        CancellationToken cancellationToken)
    {
        bool found;
        try
        {
            found = (await _client.CheckAsync(email, password, null, cancellationToken)).Found;
        }
        catch (DarkStrataException ex) when (options.FailOpen)
        {
            _logger.LogWarning(ex, "DarkStrata credential check failed ({Code}); allowing because FailOpen is enabled", ex.Code);
            return false;
        }

        if (found)
        {
            _logger.LogWarning("Compromised credential detected for {Email} via {Source}", email, source);
            await _events.PublishAsync(new CompromisedCredentialDetectedNotification(source, email, userId), cancellationToken);
        }

        return found;
    }

    /// <summary>Request-cache key. The password is hashed so it is never held as a cache key.</summary>
    private static string CacheKey(CompromisedCredentialSource source, string email, string password)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes($"{source}\n{email}\n{password}"));
        return $"DarkStrata.CredentialCheck:{Convert.ToHexString(digest)}";
    }
}
