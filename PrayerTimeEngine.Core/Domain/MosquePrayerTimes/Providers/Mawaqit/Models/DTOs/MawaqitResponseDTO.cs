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

                LocalTime fajr = localTimeParser.Parse(prayerTimeDay[0]).Value;
                LocalTime shuruq = localTimeParser.Parse(prayerTimeDay[1]).Value;
                LocalTime dhuhr = localTimeParser.Parse(prayerTimeDay[2]).Value;
                LocalTime asr = localTimeParser.Parse(prayerTimeDay[3]).Value;
                LocalTime maghrib = localTimeParser.Parse(prayerTimeDay[4]).Value;
                LocalTime isha = localTimeParser.Parse(prayerTimeDay[5]).Value;

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
                    FajrCongregation = fajr.PlusMinutes(int.Parse(iqamaTimeValueForDay[0])),
                    DhuhrCongregation = dhuhr.PlusMinutes(int.Parse(iqamaTimeValueForDay[1])),
                    AsrCongregation = asr.PlusMinutes(int.Parse(iqamaTimeValueForDay[2])),
                    MaghribCongregation = maghrib.PlusMinutes(int.Parse(iqamaTimeValueForDay[3])),
                    IshaCongregation = isha.PlusMinutes(int.Parse(iqamaTimeValueForDay[4]))
                };
            }
        }
    }
}