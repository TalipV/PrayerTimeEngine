using NodaTime.Text;
using NodaTime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.JsonConverters;

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

        // Sometimes the value "Sabah" is sent like that... they obviously mean 06:00
        if (timeString == "05:60")
            return new LocalTime(06, 00);

        throw new JsonException($"Failed to parse {timeString} as LocalTime.");
    }

    public override void Write(Utf8JsonWriter writer, LocalTime value, JsonSerializerOptions options)
    {
        string timeString = LongTimePattern.Format(value);
        writer.WriteStringValue(timeString);
    }
}