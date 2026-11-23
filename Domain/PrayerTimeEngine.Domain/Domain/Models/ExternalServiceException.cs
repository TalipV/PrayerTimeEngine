namespace PrayerTimeEngine.Core.Domain.Models;

/// <summary>What went wrong with a request to an external service.</summary>
public enum EExternalServiceFailure
{
    /// <summary>The service refused because of too many requests (HTTP 429), also after the automatic retries.</summary>
    RateLimited,

    /// <summary>The service did not answer in time.</summary>
    Timeout,

    /// <summary>The app pauses requests to this service for a moment because many of them failed shortly before.</summary>
    TemporarilyPaused,

    /// <summary>The service reported an error of its own (HTTP 5xx).</summary>
    ServerError,

    /// <summary>The service does not know the requested resource (HTTP 404).</summary>
    NotFound,

    /// <summary>The service rejected the request (other HTTP 4xx).</summary>
    RequestRejected,

    /// <summary>No connection to the service (no internet, DNS, TLS, ...).</summary>
    Unreachable,

    /// <summary>The service answered, but not in the expected format.</summary>
    InvalidResponse,
}

/// <summary>
/// A failed request to an external service (prayer time APIs, place service, ...), already
/// classified, so every layer up to the UI can tell exactly which service failed and why
/// without knowing anything about HTTP, Refit or Polly.
/// </summary>
public class ExternalServiceException(
        string serviceName,
        EExternalServiceFailure failure,
        int? httpStatusCode = null,
        DateTimeOffset? availableAgainAt = null,
        string? details = null,
        Exception? innerException = null)
    : Exception(CreateMessage(serviceName, failure, httpStatusCode, availableAgainAt, details), innerException)
{
    /// <summary>Name of the failed service, e.g. "LocationIQ" or "Semerkand".</summary>
    public string ServiceName { get; } = serviceName;

    public EExternalServiceFailure Failure { get; } = failure;

    public int? HttpStatusCode { get; } = httpStatusCode;

    /// <summary>When the service may be asked again (only known for rate limits and pauses).</summary>
    public DateTimeOffset? AvailableAgainAt { get; } = availableAgainAt;

    private static string CreateMessage(
        string serviceName,
        EExternalServiceFailure failure,
        int? httpStatusCode,
        DateTimeOffset? availableAgainAt,
        string? details)
    {
        string description = failure switch
        {
            EExternalServiceFailure.RateLimited => "too many requests",
            EExternalServiceFailure.Timeout => "no answer in time",
            EExternalServiceFailure.TemporarilyPaused => "requests paused after repeated failures",
            EExternalServiceFailure.ServerError => "server error",
            EExternalServiceFailure.NotFound => "not found",
            EExternalServiceFailure.RequestRejected => "request rejected",
            EExternalServiceFailure.Unreachable => "not reachable",
            EExternalServiceFailure.InvalidResponse => "unexpected answer",
            _ => failure.ToString(),
        };

        string message = $"{serviceName}: {description}";

        if (httpStatusCode is int statusCode)
        {
            message += $" (HTTP {statusCode})";
        }

        if (availableAgainAt is DateTimeOffset availableAgain)
        {
            message += $", available again at {availableAgain.ToLocalTime():HH:mm:ss}";
        }

        if (!string.IsNullOrWhiteSpace(details))
        {
            message += $" - {details}";
        }

        return message;
    }

    /// <summary>The first <see cref="ExternalServiceException"/> in the chain of the given exception and its inner exceptions.</summary>
    public static ExternalServiceException? FindIn(Exception? exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is ExternalServiceException externalServiceException)
            {
                return externalServiceException;
            }
        }

        return null;
    }
}
