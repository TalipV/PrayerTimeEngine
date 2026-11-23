using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Core.Common.Http;

/// <summary>
/// The limits of one external API: the experience values the throttle starts with.
/// </summary>
/// <param name="ApiName">Name for logs and error messages, e.g. "LocationIQ".</param>
/// <param name="MinRequestInterval">Smallest gap between two requests to this API (0 = no spacing).</param>
/// <param name="MaxRequestInterval">The gap never grows beyond this, however often the API complains.</param>
/// <param name="DefaultCooldown">Pause after the first 429 when the API does not say how long to wait.</param>
/// <param name="MaxCooldown">Upper bound of the pause (repeated 429s double it up to this value).</param>
/// <param name="MaxWaitBeforeFailing">
/// A request waits at most this long for a running cooldown; with a longer one it fails right away
/// with <see cref="ApiCooldownException"/> instead of blocking the app.
/// </param>
public sealed record ApiThrottleOptions(
    string ApiName,
    TimeSpan MinRequestInterval,
    TimeSpan MaxRequestInterval,
    TimeSpan DefaultCooldown,
    TimeSpan MaxCooldown,
    TimeSpan MaxWaitBeforeFailing);

/// <summary>
/// Thrown when an API is in a cooldown that lasts longer than a request may wait for it
/// (e.g. after a daily limit was reached). Deliberately no <see cref="HttpRequestException"/>,
/// so the resilience pipeline does not retry it.
/// </summary>
public sealed class ApiCooldownException(string apiName, DateTimeOffset availableAgainAt)
    : Exception($"{apiName}: too many requests, available again at {availableAgainAt.ToLocalTime():HH:mm:ss}.")
{
    public string ApiName { get; } = apiName;
    public DateTimeOffset AvailableAgainAt { get; } = availableAgainAt;
}

/// <summary>
/// Client side rate limiting for one external API, shared by all requests to it (singleton per API).
/// <list type="bullet">
/// <item>Spaces the requests by an adaptive minimum interval, so the API's limit is not hit in the first place.</item>
/// <item>On a 429 (or a 503 with Retry-After) every further request to this API waits for the cooldown
///       (Retry-After or RateLimit-Reset of the response, otherwise an exponentially growing default),
///       instead of hammering the API, and the minimum interval grows (experience value).</item>
/// <item>A long run of successes slowly lowers the interval back to the configured start value.</item>
/// <item>Headers like <c>RateLimit-Remaining: 0</c> start a cooldown before the API has to refuse a request.</item>
/// </list>
/// The retries themselves are done by the resilience pipeline around this handler (each attempt passes it again).
/// </summary>
public sealed class ApiThrottle(ApiThrottleOptions options, TimeProvider timeProvider, ILogger<ApiThrottle> logger)
{
    private const int SUCCESSES_PER_RELAXATION = 20;
    private const double RELAXATION_FACTOR = 0.8;

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Lock _stateLock = new();

    private TimeSpan _currentInterval = options.MinRequestInterval;
    private DateTimeOffset _nextRequestAllowedAt = DateTimeOffset.MinValue;
    private DateTimeOffset _cooldownUntil = DateTimeOffset.MinValue;
    private int _consecutiveRateLimits;
    private int _successesSinceRelaxation;

    public ApiThrottleOptions Options { get; } = options;

    /// <summary>The current, learned minimum gap between two requests.</summary>
    public TimeSpan CurrentInterval
    {
        get { lock (_stateLock) { return _currentInterval; } }
    }

    /// <summary>End of the current cooldown (in the past if there is none).</summary>
    public DateTimeOffset CooldownUntil
    {
        get { lock (_stateLock) { return _cooldownUntil; } }
    }

    /// <summary>Waits until this API may be asked again and reserves the slot for the caller.</summary>
    public async Task WaitForSlotAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            DateTimeOffset allowedAt;
            DateTimeOffset cooldownUntil;
            lock (_stateLock)
            {
                cooldownUntil = _cooldownUntil;
                allowedAt = _nextRequestAllowedAt > cooldownUntil ? _nextRequestAllowedAt : cooldownUntil;
            }

            TimeSpan waitTime = allowedAt - now;
            if (cooldownUntil > now && cooldownUntil - now > Options.MaxWaitBeforeFailing)
            {
                throw new ApiCooldownException(Options.ApiName, cooldownUntil);
            }

            if (waitTime > TimeSpan.Zero)
            {
                logger.LogDebug("{ApiName}: waiting {WaitMs} ms before the next request", Options.ApiName, (int)waitTime.TotalMilliseconds);
                await Task.Delay(waitTime, timeProvider, cancellationToken).ConfigureAwait(false);
                now = timeProvider.GetUtcNow();
            }

            lock (_stateLock)
            {
                _nextRequestAllowedAt = now + _currentInterval;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Learns from the response: cooldown and larger interval on rate limits, relaxation on successes.</summary>
    public void RegisterResponse(HttpResponseMessage response)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        TimeSpan? serverWaitTime = GetServerWaitTime(response, now);

        bool isRateLimited =
            response.StatusCode == HttpStatusCode.TooManyRequests
            || (response.StatusCode == HttpStatusCode.ServiceUnavailable && serverWaitTime is not null);

        lock (_stateLock)
        {
            if (isRateLimited)
            {
                _consecutiveRateLimits++;
                _successesSinceRelaxation = 0;

                TimeSpan cooldown = serverWaitTime ?? Multiply(Options.DefaultCooldown, Math.Pow(2, _consecutiveRateLimits - 1));
                cooldown = Min(cooldown, Options.MaxCooldown);

                TimeSpan grownInterval = _currentInterval == TimeSpan.Zero
                    ? TimeSpan.FromMilliseconds(100)
                    : Multiply(_currentInterval, 2);
                _currentInterval = Min(grownInterval, Options.MaxRequestInterval);

                if (now + cooldown > _cooldownUntil)
                {
                    _cooldownUntil = now + cooldown;
                }

                logger.LogWarning(
                    "{ApiName} answered {StatusCode} ({Count}x in a row): cooldown {CooldownMs} ms, request interval now {IntervalMs} ms",
                    Options.ApiName, (int)response.StatusCode, _consecutiveRateLimits, (int)cooldown.TotalMilliseconds, (int)_currentInterval.TotalMilliseconds);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            _consecutiveRateLimits = 0;

            // the API announces that its quota is used up: pause before it has to refuse a request
            if (IsQuotaExhausted(response) && serverWaitTime is TimeSpan resetTime)
            {
                DateTimeOffset resetAt = now + Min(resetTime, Options.MaxCooldown);
                if (resetAt > _cooldownUntil)
                {
                    _cooldownUntil = resetAt;
                    logger.LogInformation("{ApiName}: quota used up, pausing for {CooldownMs} ms", Options.ApiName, (int)(resetAt - now).TotalMilliseconds);
                }
            }

            if (++_successesSinceRelaxation >= SUCCESSES_PER_RELAXATION && _currentInterval > Options.MinRequestInterval)
            {
                _successesSinceRelaxation = 0;
                _currentInterval = Max(Multiply(_currentInterval, RELAXATION_FACTOR), Options.MinRequestInterval);
                logger.LogDebug("{ApiName}: request interval relaxed to {IntervalMs} ms", Options.ApiName, (int)_currentInterval.TotalMilliseconds);
            }
        }
    }

    private static bool IsQuotaExhausted(HttpResponseMessage response)
    {
        return TryGetHeaderNumber(response.Headers, "RateLimit-Remaining", out double remaining) && remaining <= 0;
    }

    /// <summary>Retry-After (seconds or date) or, as a fallback, RateLimit-Reset (seconds).</summary>
    private static TimeSpan? GetServerWaitTime(HttpResponseMessage response, DateTimeOffset now)
    {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is TimeSpan delta)
        {
            return delta;
        }

        if (retryAfter?.Date is DateTimeOffset date)
        {
            return date > now ? date - now : TimeSpan.Zero;
        }

        if (TryGetHeaderNumber(response.Headers, "RateLimit-Reset", out double resetSeconds) && resetSeconds >= 0)
        {
            return TimeSpan.FromSeconds(resetSeconds);
        }

        return null;
    }

    private static bool TryGetHeaderNumber(HttpResponseHeaders headers, string name, out double value)
    {
        value = 0;
        return headers.TryGetValues(name, out IEnumerable<string>? values)
            && double.TryParse(values.FirstOrDefault(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static TimeSpan Multiply(TimeSpan timeSpan, double factor) => TimeSpan.FromTicks((long)Math.Min(timeSpan.Ticks * factor, TimeSpan.MaxValue.Ticks / 2));
    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
    private static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;
}

/// <summary>
/// Puts an <see cref="ApiThrottle"/> in front of every single HTTP attempt. Sits inside the
/// resilience pipeline, so retried attempts are throttled as well.
/// </summary>
public sealed class ApiThrottlingHandler(ApiThrottle throttle) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await throttle.WaitForSlotAsync(cancellationToken).ConfigureAwait(false);
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        throttle.RegisterResponse(response);
        return response;
    }
}
