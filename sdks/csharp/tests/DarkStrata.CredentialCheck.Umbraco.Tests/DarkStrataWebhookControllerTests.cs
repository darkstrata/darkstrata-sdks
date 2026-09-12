using System.Text;
using DarkStrata.CredentialCheck.Umbraco;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace DarkStrata.CredentialCheck.Umbraco.Tests;

public class DarkStrataWebhookControllerTests
{
    private const string Secret = "s3cret";
    private const string Body = """{"event":"alert.stealer_log.created","data":{"alert":{"id":"abc"}},"timestamp":"2026-09-12T00:00:00Z","webhook_id":"w1"}""";

    private static DarkStrataWebhookController Controller(IEventAggregator events, string? configuredSecret, string? presentedSecret, string body = Body)
    {
        var http = new DefaultHttpContext();
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        if (presentedSecret is not null)
        {
            http.Request.Headers[DarkStrataOptions.WebhookSecretHeader] = presentedSecret;
        }

        return new DarkStrataWebhookController(events, TestSupport.Options(new DarkStrataOptions { WebhookSecret = configuredSecret }))
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
    }

    [Fact]
    public async Task Valid_secret_publishes_notification_and_returns_202()
    {
        var events = new Mock<IEventAggregator>();

        var result = await Controller(events.Object, Secret, Secret).Receive(CancellationToken.None);

        Assert.IsType<AcceptedResult>(result);
        events.Verify(
            e => e.PublishAsync(
                It.Is<DarkStrataAlertReceivedNotification>(n => n.EventKey == "alert.stealer_log.created" && n.Payload.GetProperty("webhook_id").GetString() == "w1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("wrong")]
    [InlineData(null)]
    public async Task Bad_or_missing_secret_returns_401(string? presented)
    {
        var events = new Mock<IEventAggregator>();

        var result = await Controller(events.Object, Secret, presented).Receive(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
        events.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Unconfigured_secret_returns_503()
    {
        var result = await Controller(Mock.Of<IEventAggregator>(), null, Secret).Receive(CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Malformed_body_returns_400()
    {
        var result = await Controller(Mock.Of<IEventAggregator>(), Secret, Secret, "{not json").Receive(CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
    }
}
