using DarkStrata.CredentialCheck.Umbraco;
using Moq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace DarkStrata.CredentialCheck.Umbraco.Tests;

public class CompromisedCredentialServiceTests
{
    // Umbraco checks the same credentials twice per backoffice login; the key owner must only pay once.
    [Fact]
    public async Task Same_credentials_are_checked_once_per_request()
    {
        var client = TestSupport.Client(found: true);
        var events = new Mock<IEventAggregator>();
        var service = TestSupport.Service(client.Object, new DarkStrataOptions { ApiKey = TestSupport.ApiKey }, events.Object);

        Assert.True(await service.IsCompromisedAsync(CompromisedCredentialSource.BackOfficeLogin, "a@example.com", "hunter2", "1"));
        Assert.True(await service.IsCompromisedAsync(CompromisedCredentialSource.BackOfficeLogin, "a@example.com", "hunter2", "1"));

        client.Verify(c => c.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
        events.Verify(
            e => e.PublishAsync(It.IsAny<CompromisedCredentialDetectedNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Different_credentials_are_checked_separately()
    {
        var client = TestSupport.Client(found: false);
        var service = TestSupport.Service(client.Object, new DarkStrataOptions { ApiKey = TestSupport.ApiKey });

        await service.IsCompromisedAsync(CompromisedCredentialSource.MemberLogin, "a@example.com", "hunter2", "1");
        await service.IsCompromisedAsync(CompromisedCredentialSource.MemberLogin, "a@example.com", "hunter3", "1");

        client.Verify(c => c.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // Outside an HTTP request there is no request cache; the check must still run.
    [Fact]
    public async Task Works_without_a_request_cache()
    {
        var client = TestSupport.Client(found: true);
        var service = TestSupport.Service(
            client.Object,
            new DarkStrataOptions { ApiKey = TestSupport.ApiKey },
            requestCache: AppCaches.Disabled.RequestCache);

        Assert.True(await service.IsCompromisedAsync(CompromisedCredentialSource.MemberPassword, "a@example.com", "hunter2", "1"));
    }
}
