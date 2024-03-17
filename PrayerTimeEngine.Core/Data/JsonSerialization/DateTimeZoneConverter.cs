using NodaTime;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Data.JsonSerialization
{
    public class DateTimeZoneConverter : JsonConverter<DateTimeZone>
    {
        public override DateTimeZone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string timezoneString = reader.GetString();

            if (string.IsNullOrWhiteSpace(timezoneString))
            {
                return null;
            }

            try
            {
                return DateTimeZoneProviders.Tzdb[timezoneString];
            }
            catch
            {
                try
                {
                    return DateTimeZoneProviders.Bcl[timezoneString];
                }
                catch { }
            }

            throw new JsonException($"Failed to parse '{timezoneString}' as DateTimeZone [TZDB and BCL provider].");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeZone value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }

    }
}
