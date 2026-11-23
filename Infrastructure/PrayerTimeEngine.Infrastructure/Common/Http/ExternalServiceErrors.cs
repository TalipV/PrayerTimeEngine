using System.Net;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PrayerTimeEngine.Core.Domain.Models;
using Refit;

namespace PrayerTimeEngine.Core.Common.Http;

/// <summary>
/// Translates the technical failures of the HTTP stack (status codes, Polly timeouts and circuit
/// breaker, network errors, unreadable answers) into <see cref="ExternalServiceException"/>s that
/// name the service and the kind of failure. Used by every HTTP based adapter, so the inner rings
/// and the UI get the same precise information for every service.
/// </summary>
public static class ExternalServiceErrors
{
    /// <summary>
    /// Refit settings of a client whose exceptions are translated: Refit's own exception factories
    /// are replaced, everything else (e.g. the source generated JSON context) stays as given.
    /// </summary>
    public static RefitSettings Configure(RefitSettings settings, string serviceName, ApiThrottle? throttle)
    {
        settings.ExceptionFactory = async response =>
        {
            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            // Refit's exception (with the response content) stays available as inner exception for the logs
            ApiException? apiException = response.RequestMessage is HttpRequestMessage request
                ? await ApiException.Create(request, request.Method, response, settings).ConfigureAwait(false)
                : null;

            return FromResponse(serviceName, response, throttle, apiException);
        };

        settings.TransportExceptionFactory = (request, exception, cancellationToken) =>
            FromTransportException(serviceName, exception, cancellationToken);

        settings.DeserializationExceptionFactory = (response, exception) =>
            ValueTask.FromResult<Exception>(new ExternalServiceException(
                serviceName, EExternalServiceFailure.InvalidResponse, details: exception.Message, innerException: exception));

        return settings;
    }

    /// <summary>The exception for an unsuccessful response (after all retries).</summary>
    public static ExternalServiceException FromResponse(string serviceName, HttpResponseMessage response, ApiThrottle? throttle, Exception? innerException = null)
    {
        int statusCode = (int)response.StatusCode;

        EExternalServiceFailure failure = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => EExternalServiceFailure.RateLimited,
            HttpStatusCode.NotFound => EExternalServiceFailure.NotFound,
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => EExternalServiceFailure.Timeout,
            _ when statusCode >= 500 => EExternalServiceFailure.ServerError,
            _ => EExternalServiceFailure.RequestRejected,
        };

        DateTimeOffset? availableAgainAt = null;
        if (failure == EExternalServiceFailure.RateLimited && throttle is not null && throttle.CooldownUntil > DateTimeOffset.UtcNow)
        {
            availableAgainAt = throttle.CooldownUntil;
        }

        return new ExternalServiceException(
            serviceName,
            failure,
            httpStatusCode: statusCode,
            availableAgainAt: availableAgainAt,
            details: response.ReasonPhrase,
            innerException: innerException);
    }

    /// <summary>
    /// The exception for a request that got no usable response. A real cancellation by the caller
    /// is passed through unchanged, so the callers can still tell it apart from a failure.
    /// </summary>
    public static Exception FromTransportException(string serviceName, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return exception;
        }

        return exception switch
        {
            ExternalServiceException => exception,

            // the circuit breaker of the resilience pipeline is open (it closes again after a few seconds)
            BrokenCircuitException => new ExternalServiceException(
                serviceName, EExternalServiceFailure.TemporarilyPaused, innerException: exception),

            // Polly's attempt/total timeout or HttpClient.Timeout (a TaskCanceledException without cancellation by the caller)
            TimeoutRejectedException or OperationCanceledException => new ExternalServiceException(
                serviceName, EExternalServiceFailure.Timeout, innerException: exception),

            HttpRequestException httpRequestException => new ExternalServiceException(
                serviceName,
                EExternalServiceFailure.Unreachable,
                details: httpRequestException.HttpRequestError == HttpRequestError.Unknown ? exception.Message : httpRequestException.HttpRequestError.ToString(),
                innerException: exception),

            JsonException => new ExternalServiceException(
                serviceName, EExternalServiceFailure.InvalidResponse, details: exception.Message, innerException: exception),

            _ => new ExternalServiceException(
                serviceName, EExternalServiceFailure.Unreachable, details: exception.Message, innerException: exception),
        };
    }
}
