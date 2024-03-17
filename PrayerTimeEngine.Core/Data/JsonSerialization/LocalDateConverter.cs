using NodaTime.Text;
using NodaTime;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;
using NodaTime.Extensions;

namespace PrayerTimeEngine.Core.Data.JsonSerialization
{
    public class LocalDateConverter : JsonConverter<LocalDate>
    {
        public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string dateString = reader.GetString();

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, out DateTime parsedDateTime))
            {
                return parsedDateTime.ToLocalDateTime().Date;
            }
            else if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, out DateTimeOffset parsedDateTimeOffset))
            {
                return parsedDateTimeOffset.Date.ToLocalDateTime().Date;
            }

            throw new JsonException($"Failed to parse {dateString} as LocalDate.");
        }

        public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
        {
            string dateString = LocalDatePattern.Iso.Format(value);
            writer.WriteStringValue(dateString);
        }
    }
}
