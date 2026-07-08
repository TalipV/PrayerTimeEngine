using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.JsonConverters;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;

public class MawaqitResponseDTO
{
    [JsonPropertyName("timeDisplayFormat")]
    public string TimeDisplayFormat { get; set; }   // e.g. "24"

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }

    [JsonPropertyName("timezone")]
    public string TimeZone { get; set; }

    [JsonPropertyName("latitude")]
    public decimal Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal Longitude { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("jumua")]
    public LocalTime? Jumuah { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("jumua2")]
    public LocalTime? Jumuah2 { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("jumua3")]
    public LocalTime? Jumuah3 { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("shuruq")]
    public LocalTime? Shuruq { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("aidPrayerTime")]
    public LocalTime? EidPrayerTime { get; set; }

    [JsonConverter(typeof(LocalTimeConverter))]
    [JsonPropertyName("aidPrayerTime2")]
    public LocalTime? EidPrayerTime2 { get; set; }

    [JsonPropertyName("calendar")]
    public List<Dictionary<string, List<string>>> PrayerTimeDays { get; set; }

    [JsonPropertyName("iqamaCalendar")]
    public List<Dictionary<string, List<string>>> IqamahTimeValuesForDays { get; set; }

    public IEnumerable<MawaqitMosqueDailyPrayerTimes> ToMawaqitPrayerTimes(int year, string externalID)
    {
        if (PrayerTimeDays.Count != 12 || IqamahTimeValuesForDays.Count != 12)
        {
            throw new Exception("Expected 12 months for both lists");
        }

        var firstDayOfCurrentYear = new LocalDate(year, 1, 1);

        for (int monthCounter = 0; monthCounter < 12; monthCounter++)
        {
            // times per month
            Dictionary<string, List<string>> prayerTimeDaysItemOfMonth = PrayerTimeDays.ElementAt(monthCounter);
            Dictionary<string, List<string>> iqamahTimeValuesForDaysItem = IqamahTimeValuesForDays.ElementAt(monthCounter);

            var localTimeParser = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
            LocalDate firstDayOfThisMonth = firstDayOfCurrentYear.PlusMonths(monthCounter);
            LocalDate lastDayOfThisMonth = firstDayOfThisMonth.PlusMonths(1).PlusDays(-1);

            for (int dayCounter = 1; dayCounter <= prayerTimeDaysItemOfMonth.Count; dayCounter++)
            {
                // unfortunately, the API does provide broken data like that
                if (dayCounter > lastDayOfThisMonth.Day)
                {
                    break;
                }

                LocalDate currentDay = firstDayOfThisMonth.PlusDays(dayCounter - 1);

                List<string> prayerTimeDay = prayerTimeDaysItemOfMonth["" + dayCounter];
                List<string> iqamaTimeValueForDay = iqamahTimeValuesForDaysItem["" + dayCounter];

                if (prayerTimeDay.Count != 6 || iqamaTimeValueForDay.Count != 5)
                {
                    throw new Exception("Expected 6 times and 5 iqamah durations");
                }

                LocalTime? fajr = parseTimeOrNull(localTimeParser, prayerTimeDay[0]);
                LocalTime? shuruq = parseTimeOrNull(localTimeParser, prayerTimeDay[1]);
                LocalTime? dhuhr = parseTimeOrNull(localTimeParser, prayerTimeDay[2]);
                LocalTime? asr = parseTimeOrNull(localTimeParser, prayerTimeDay[3]);
                LocalTime? maghrib = parseTimeOrNull(localTimeParser, prayerTimeDay[4]);
                LocalTime? isha = parseTimeOrNull(localTimeParser, prayerTimeDay[5]);

                yield return new MawaqitMosqueDailyPrayerTimes
                {
                    ExternalID = externalID,
                    Date = currentDay,
                    Fajr = fajr,
                    Shuruq = shuruq,
                    Dhuhr = dhuhr,
                    Asr = asr,
                    Maghrib = maghrib,
                    Isha = isha,
                    Jumuah = Jumuah,
                    Jumuah2 = Jumuah2,
                    FajrCongregation = getCongregationTimeOrNull(fajr, iqamaTimeValueForDay[0]),
                    DhuhrCongregation = getCongregationTimeOrNull(dhuhr, iqamaTimeValueForDay[1]),
                    AsrCongregation = getCongregationTimeOrNull(asr, iqamaTimeValueForDay[2]),
                    MaghribCongregation = getCongregationTimeOrNull(maghrib, iqamaTimeValueForDay[3]),
                    IshaCongregation = getCongregationTimeOrNull(isha, iqamaTimeValueForDay[4])
                };
            }
        }
    }

    private static LocalTime? parseTimeOrNull(LocalTimePattern localTimeParser, string timeText)
    {
        if (string.IsNullOrWhiteSpace(timeText))
            return null;

        ParseResult<LocalTime> parseResult = localTimeParser.Parse(timeText);
        return parseResult.Success ? parseResult.Value : null;
    }

    private static LocalTime? getCongregationTimeOrNull(LocalTime? baseTime, string offsetMinutesText)
    {
        if (baseTime is null || !int.TryParse(offsetMinutesText, out int offsetMinutes))
            return null;

        return baseTime.Value.PlusMinutes(offsetMinutes);
    }
}