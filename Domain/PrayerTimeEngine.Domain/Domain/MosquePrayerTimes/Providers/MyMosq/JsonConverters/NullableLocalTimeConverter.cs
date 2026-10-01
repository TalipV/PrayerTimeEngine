using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Text;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.JsonConverters;

public class NullableLocalTimeConverter : JsonConverter<LocalTime?>
{
    private static readonly LocalTimePattern s_longTimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");
    private static readonly LocalTimePattern s_shortTimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");

    public override LocalTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string timeString = reader.GetString();

        if (string.IsNullOrEmpty(timeString))
        {
            return null;
        }

        if (s_longTimePattern.Parse(timeString).TryGetValue(LocalTime.MinValue, out LocalTime parsedLocalTime))
        {
            return parsedLocalTime;
        }
        else if (s_shortTimePattern.Parse(timeString).TryGetValue(LocalTime.MinValue, out parsedLocalTime))
        {
            return parsedLocalTime;
        }

        throw new JsonException($"Failed to parse {timeString} as LocalTime.");
    }

    public override void Write(Utf8JsonWriter writer, LocalTime? value, JsonSerializerOptions options)
    {
        string timeString = value != null
            ? s_longTimePattern.Format(value.Value)
            : null;
        writer.WriteStringValue(timeString);
    }
}