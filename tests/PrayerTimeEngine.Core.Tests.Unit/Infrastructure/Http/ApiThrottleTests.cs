using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging.Abstractions;
using PrayerTimeEngine.Core.Common.Http;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Infrastructure.Http;

public class ApiThrottleTests : BaseTest
{
    private sealed class ManualTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly ApiThrottleOptions s_options = new(
        ApiName: "TestApi",
        MinRequestInterval: TimeSpan.FromMilliseconds(500),
        MaxRequestInterval: TimeSpan.FromSeconds(3),
        DefaultCooldown: TimeSpan.FromSeconds(1),
        MaxCooldown: TimeSpan.FromMinutes(5),
        MaxWaitBeforeFailing: TimeSpan.FromSeconds(15));

    private readonly ManualTimeProvider _timeProvider = new(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero));

    private ApiThrottle CreateThrottle(ApiThrottleOptions? options = null)
        => new(options ?? s_options, _timeProvider, NullLogger<ApiThrottle>.Instance);

    private static HttpResponseMessage CreateTooManyRequestsResponse(TimeSpan? retryAfter = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfter is TimeSpan delta)
        {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(delta);
        }

        return response;
    }

    [Fact]
    public async Task RegisterResponse_TooManyRequestsWithLongRetryAfter_NextRequestFailsFastWithCooldownException()
    {
        // ARRANGE
        ApiThrottle throttle = CreateThrottle();

        // ACT
        throttle.RegisterResponse(CreateTooManyRequestsResponse(TimeSpan.FromSeconds(60)));
        Func<Task> nextRequest = () => throttle.WaitForSlotAsync(default);

        // ASSERT
        throttle.CooldownUntil.Should().Be(_timeProvider.Now.AddSeconds(60));
        throttle.CurrentInterval.Should().Be(TimeSpan.FromSeconds(1));
        (await nextRequest.Should().ThrowAsync<ApiCooldownException>())
            .Which.AvailableAgainAt.Should().Be(_timeProvider.Now.AddSeconds(60));
    }

    [Fact]
    public void RegisterResponse_RepeatedTooManyRequestsWithoutRetryAfter_CooldownAndIntervalGrowUpToTheirLimits()
    {
        // ARRANGE
        ApiThrottle throttle = CreateThrottle();

        // ACT & ASSERT
        throttle.RegisterResponse(CreateTooManyRequestsResponse());
        throttle.CooldownUntil.Should().Be(_timeProvider.Now.AddSeconds(1));
        throttle.CurrentInterval.Should().Be(TimeSpan.FromSeconds(1));

        throttle.RegisterResponse(CreateTooManyRequestsResponse());
        throttle.CooldownUntil.Should().Be(_timeProvider.Now.AddSeconds(2));
        throttle.CurrentInterval.Should().Be(TimeSpan.FromSeconds(2));

        throttle.RegisterResponse(CreateTooManyRequestsResponse());
        throttle.CooldownUntil.Should().Be(_timeProvider.Now.AddSeconds(4));
        throttle.CurrentInterval.Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void RegisterResponse_ManySuccessesAfterRateLimit_IntervalRelaxesBackToTheConfiguredMinimum()
    {
        // ARRANGE
        ApiThrottle throttle = CreateThrottle();
        throttle.RegisterResponse(CreateTooManyRequestsResponse());
        throttle.CurrentInterval.Should().Be(TimeSpan.FromSeconds(1));

        // ACT
        for (int i = 0; i < 200; i++)
        {
            throttle.RegisterResponse(new HttpResponseMessage(HttpStatusCode.OK));
        }

        // ASSERT
        throttle.CurrentInterval.Should().Be(s_options.MinRequestInterval);
    }

    [Fact]
    public void RegisterResponse_SuccessAnnouncingExhaustedQuota_StartsCooldownBeforeTheApiRefuses()
    {
        // ARRANGE
        ApiThrottle throttle = CreateThrottle();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("RateLimit-Remaining", "0");
        response.Headers.Add("RateLimit-Reset", "7");

        // ACT
        throttle.RegisterResponse(response);

        // ASSERT
        throttle.CooldownUntil.Should().Be(_timeProvider.Now.AddSeconds(7));
        throttle.CurrentInterval.Should().Be(s_options.MinRequestInterval);
    }

    [Fact]
    public void RegisterResponse_SuccessWithRemainingQuota_NoCooldown()
    {
        // ARRANGE
        ApiThrottle throttle = CreateThrottle();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("RateLimit-Remaining", "99");
        response.Headers.Add("RateLimit-Reset", "1");

        // ACT
        throttle.RegisterResponse(response);

        // ASSERT
        throttle.CooldownUntil.Should().BeBefore(_timeProvider.Now);
    }

    [Fact]
    public async Task WaitForSlotAsync_TwoRequests_SecondWaitsForTheMinimumInterval()
    {
        // ARRANGE
        var throttle = new ApiThrottle(
            s_options with { MinRequestInterval = TimeSpan.FromMilliseconds(300) },
            TimeProvider.System,
            NullLogger<ApiThrottle>.Instance);

        // ACT
        var stopwatch = Stopwatch.StartNew();
        await throttle.WaitForSlotAsync(default);
        await throttle.WaitForSlotAsync(default);
        stopwatch.Stop();

        // ASSERT
        stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(250));
    }
}
