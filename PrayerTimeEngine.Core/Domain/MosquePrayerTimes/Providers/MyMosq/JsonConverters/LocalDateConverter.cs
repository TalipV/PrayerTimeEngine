using NodaTime;
using NodaTime.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.JsonConverters;
public class LocalDateConverter : JsonConverter<LocalDate>
{
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.CreateWithInvariantCulture("dd.MM.yyyy");

    public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string dateString = reader.GetString();

        if (DatePattern.Parse(dateString).TryGetValue(LocalDate.MinIsoValue, out LocalDate parsedDate))
        {
            return parsedDate;
        }

        throw new JsonException($"Failed to parse {dateString} as LocalDate.");
    }

    public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
    {
        string dateString = LocalDatePattern.Iso.Format(value);
        writer.WriteStringValue(dateString);
    }
}
