﻿using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Data.JsonSerialization;

public class OffsetDateTimeConverter : JsonConverter<OffsetDateTime>
{
    public override OffsetDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string offsetDateTimeString = reader.GetString();

        if (DateTimeOffset.TryParse(offsetDateTimeString, CultureInfo.InvariantCulture, out DateTimeOffset parsedDateTimeOffset))
        {
            return parsedDateTimeOffset.ToOffsetDateTime();
        }

        throw new JsonException($"Failed to parse '{offsetDateTimeString}' as OffsetDateTime [TZDB and BCL provider].");
    }

    public override void Write(Utf8JsonWriter writer, OffsetDateTime value, JsonSerializerOptions options)
    {
        string offsetDateTimeString = OffsetDateTimePattern.GeneralIso.Format(value);
        writer.WriteStringValue(offsetDateTimeString);
    }
}
