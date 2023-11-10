using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Data.JsonConverter
{
    public class ZonedDateTimeConverter : JsonConverter<ZonedDateTime>
    {
        public override ZonedDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string stringValue = reader.GetString();
            return stringValue.GetZonedDateTimeFromDBColumnString();
        }

        public override void Write(Utf8JsonWriter writer, ZonedDateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.GetStringForDBColumn());
        }
    }
}
