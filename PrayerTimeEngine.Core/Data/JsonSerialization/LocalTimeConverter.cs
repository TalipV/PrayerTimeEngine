using NodaTime.Text;
using NodaTime;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Data.JsonSerialization
{
    public class LocalTimeConverter : JsonConverter<LocalTime>
    {
        private static readonly LocalTimePattern LongTimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");
        private static readonly LocalTimePattern ShortTimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");

        public override LocalTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string timeString = reader.GetString();

            if (LongTimePattern.Parse(timeString).TryGetValue(LocalTime.MinValue, out LocalTime parsedLocalTime))
            {
                return parsedLocalTime;
            }
            else if (ShortTimePattern.Parse(timeString).TryGetValue(LocalTime.MinValue, out parsedLocalTime))
            {
                return parsedLocalTime;
            }

            throw new JsonException($"Failed to parse {timeString} as LocalTime.");
        }

        public override void Write(Utf8JsonWriter writer, LocalTime value, JsonSerializerOptions options)
        {
            string timeString = LongTimePattern.Format(value);
            writer.WriteStringValue(timeString);
        }
    }
}
