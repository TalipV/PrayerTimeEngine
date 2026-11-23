using System.Globalization;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Services;

/// <summary>
/// Turns the exceptions of the inner rings into texts for the user that say which service failed,
/// why, and when the app tries again. The source (provider or mosque app) the problem belongs to is
/// shown by the caller, e.g. as the header of the entry behind the warning sign.
/// </summary>
public static class ProblemMessages
{
    /// <summary>The complete explanation, e.g. for the list behind the warning sign.</summary>
    public static string Describe(Exception exception)
    {
        return exception switch
        {
            PrayerTimesNotAvailableForDateException => AppResources.MainPage_Problem_NotAvailable,
            LocationDataMissingException locationDataMissingException => DescribeMissingLocationData(locationDataMissingException),
            _ when ExternalServiceException.FindIn(exception) is ExternalServiceException serviceException => DescribeServiceFailure(serviceException) + ".",
            _ => exception.Message,
        };
    }

    /// <summary>A short reason for a failed location look up, e.g. for a toast right after selecting a place.</summary>
    public static string DescribeLookupFailureShort(Exception? error)
    {
        if (error is null)
        {
            return AppResources.MainPage_LocationUnknownShort;
        }

        return ExternalServiceException.FindIn(error) is ExternalServiceException serviceException
            ? DescribeServiceFailure(serviceException)
            : error.Message;
    }

    /// <summary>E.g. "LocationIQ meldet zu viele Anfragen (HTTP 429, wieder verfügbar ab 16:25:10)".</summary>
    public static string DescribeServiceFailure(ExternalServiceException exception)
    {
        string format = exception.Failure switch
        {
            EExternalServiceFailure.RateLimited => AppResources.ServiceFailure_RateLimited,
            EExternalServiceFailure.Timeout => AppResources.ServiceFailure_Timeout,
            EExternalServiceFailure.TemporarilyPaused => AppResources.ServiceFailure_TemporarilyPaused,
            EExternalServiceFailure.ServerError => AppResources.ServiceFailure_ServerError,
            EExternalServiceFailure.NotFound => AppResources.ServiceFailure_NotFound,
            EExternalServiceFailure.RequestRejected => AppResources.ServiceFailure_RequestRejected,
            EExternalServiceFailure.Unreachable => AppResources.ServiceFailure_Unreachable,
            EExternalServiceFailure.InvalidResponse => AppResources.ServiceFailure_InvalidResponse,
            _ => "{0}: " + exception.Failure,
        };

        List<string> details = [];
        if (exception.HttpStatusCode is int statusCode)
        {
            details.Add(string.Format(CultureInfo.CurrentCulture, AppResources.ServiceFailure_HttpStatus, statusCode));
        }

        if (exception.AvailableAgainAt is DateTimeOffset availableAgainAt && availableAgainAt > DateTimeOffset.UtcNow)
        {
            details.Add(string.Format(CultureInfo.CurrentCulture, AppResources.ServiceFailure_AvailableAgainAt, FormatTime(availableAgainAt)));
        }

        string text = string.Format(CultureInfo.CurrentCulture, format, exception.ServiceName);
        return details.Count == 0 ? text : $"{text} ({string.Join(", ", details)})";
    }

    private static string DescribeMissingLocationData(LocationDataMissingException exception)
    {
        string text =
            exception.IsPlaceUnknown
                ? string.Format(CultureInfo.CurrentCulture, AppResources.MainPage_Problem_LocationUnknown, exception.PlaceDescription)
            : exception.LastError is not null
                ? string.Format(CultureInfo.CurrentCulture, AppResources.MainPage_Problem_LocationLookupFailed, exception.PlaceDescription, DescribeLookupFailureShort(exception.LastError))
            : string.Format(CultureInfo.CurrentCulture, AppResources.MainPage_Problem_LocationLookupPending, exception.PlaceDescription);

        if (exception.NextAttemptAt is Instant nextAttemptAt)
        {
            text += " " + string.Format(CultureInfo.CurrentCulture, AppResources.MainPage_Problem_NextAttempt, FormatTime(nextAttemptAt.ToDateTimeOffset()));
        }

        return text;
    }

    private static string FormatTime(DateTimeOffset time)
    {
        DateTimeOffset localTime = time.ToLocalTime();

        // with the date only when it is not today, e.g. after a long pause
        return localTime.Date == DateTimeOffset.Now.Date
            ? localTime.ToString("HH:mm", CultureInfo.CurrentCulture)
            : localTime.ToString("dd.MM. HH:mm", CultureInfo.CurrentCulture);
    }
}
