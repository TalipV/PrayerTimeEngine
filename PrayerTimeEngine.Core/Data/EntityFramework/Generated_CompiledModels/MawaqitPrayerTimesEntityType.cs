﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;

#pragma warning disable 219, 612, 618
#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels
{
    internal partial class MawaqitPrayerTimesEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Models.Entities.MawaqitPrayerTimes",
                typeof(MawaqitPrayerTimes),
                baseEntityType);

            var iD = runtimeEntityType.AddProperty(
                "ID",
                typeof(int),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<ID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0);
            iD.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

            var asr = runtimeEntityType.AddProperty(
                "Asr",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Asr", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Asr>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            asr.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            asr.SetSentinelFromProviderValue("00:00:00");

            var asrCongregation = runtimeEntityType.AddProperty(
                "AsrCongregation",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("AsrCongregation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<AsrCongregation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            asrCongregation.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            asrCongregation.SetSentinelFromProviderValue("00:00:00");

            var date = runtimeEntityType.AddProperty(
                "Date",
                typeof(LocalDate),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Date", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Date>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            date.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalDate>(
                    (LocalDate v1, LocalDate v2) => v1.Equals(v2),
                    (LocalDate v) => v.GetHashCode(),
                    (LocalDate v) => v),
                keyComparer: new ValueComparer<LocalDate>(
                    (LocalDate v1, LocalDate v2) => v1.Equals(v2),
                    (LocalDate v) => v.GetHashCode(),
                    (LocalDate v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalDate, string>(
                    (LocalDate x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalDateFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalDate, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalDate, string>(
                        (LocalDate x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalDateFromDBColumnString())));
            date.SetSentinelFromProviderValue("01/01/0001");

            var dhuhr = runtimeEntityType.AddProperty(
                "Dhuhr",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Dhuhr", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Dhuhr>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            dhuhr.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            dhuhr.SetSentinelFromProviderValue("00:00:00");

            var dhuhrCongregation = runtimeEntityType.AddProperty(
                "DhuhrCongregation",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("DhuhrCongregation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<DhuhrCongregation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            dhuhrCongregation.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            dhuhrCongregation.SetSentinelFromProviderValue("00:00:00");

            var externalID = runtimeEntityType.AddProperty(
                "ExternalID",
                typeof(string),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("ExternalID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<ExternalID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            externalID.TypeMapping = SqliteStringTypeMapping.Default;

            var fajr = runtimeEntityType.AddProperty(
                "Fajr",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Fajr", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Fajr>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            fajr.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            fajr.SetSentinelFromProviderValue("00:00:00");

            var fajrCongregation = runtimeEntityType.AddProperty(
                "FajrCongregation",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("FajrCongregation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<FajrCongregation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            fajrCongregation.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            fajrCongregation.SetSentinelFromProviderValue("00:00:00");

            var insertInstant = runtimeEntityType.AddProperty(
                "InsertInstant",
                typeof(Instant?),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("InsertInstant", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<InsertInstant>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            insertInstant.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<Instant?>(
                    (Nullable<Instant> v1, Nullable<Instant> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<Instant> v) => v.GetHashCode(),
                    (Nullable<Instant> v) => v),
                keyComparer: new ValueComparer<Instant?>(
                    (Nullable<Instant> v1, Nullable<Instant> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<Instant> v) => v.GetHashCode(),
                    (Nullable<Instant> v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<Instant?, string>(
                    (Nullable<Instant> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                    (string x) => x != null ? (Nullable<Instant>)x.GetInstantFromDBColumnString() : null),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<Instant?, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<Instant?, string>(
                        (Nullable<Instant> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                        (string x) => x != null ? (Nullable<Instant>)x.GetInstantFromDBColumnString() : null)));

            var isha = runtimeEntityType.AddProperty(
                "Isha",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Isha", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Isha>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isha.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            isha.SetSentinelFromProviderValue("00:00:00");

            var ishaCongregation = runtimeEntityType.AddProperty(
                "IshaCongregation",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("IshaCongregation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<IshaCongregation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            ishaCongregation.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            ishaCongregation.SetSentinelFromProviderValue("00:00:00");

            var jumuah = runtimeEntityType.AddProperty(
                "Jumuah",
                typeof(LocalTime?),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Jumuah", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Jumuah>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            jumuah.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime?>(
                    (Nullable<LocalTime> v1, Nullable<LocalTime> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<LocalTime> v) => v.GetHashCode(),
                    (Nullable<LocalTime> v) => v),
                keyComparer: new ValueComparer<LocalTime?>(
                    (Nullable<LocalTime> v1, Nullable<LocalTime> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<LocalTime> v) => v.GetHashCode(),
                    (Nullable<LocalTime> v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime?, string>(
                    (Nullable<LocalTime> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                    (string x) => x != null ? (Nullable<LocalTime>)x.GetLocalTimeFromDBColumnString() : null),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime?, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime?, string>(
                        (Nullable<LocalTime> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                        (string x) => x != null ? (Nullable<LocalTime>)x.GetLocalTimeFromDBColumnString() : null)));

            var jumuah2 = runtimeEntityType.AddProperty(
                "Jumuah2",
                typeof(LocalTime?),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Jumuah2", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Jumuah2>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            jumuah2.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime?>(
                    (Nullable<LocalTime> v1, Nullable<LocalTime> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<LocalTime> v) => v.GetHashCode(),
                    (Nullable<LocalTime> v) => v),
                keyComparer: new ValueComparer<LocalTime?>(
                    (Nullable<LocalTime> v1, Nullable<LocalTime> v2) => object.Equals((object)v1, (object)v2),
                    (Nullable<LocalTime> v) => v.GetHashCode(),
                    (Nullable<LocalTime> v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime?, string>(
                    (Nullable<LocalTime> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                    (string x) => x != null ? (Nullable<LocalTime>)x.GetLocalTimeFromDBColumnString() : null),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime?, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime?, string>(
                        (Nullable<LocalTime> x) => x != null ? x.Value.GetStringForDBColumn() : null,
                        (string x) => x != null ? (Nullable<LocalTime>)x.GetLocalTimeFromDBColumnString() : null)));

            var maghrib = runtimeEntityType.AddProperty(
                "Maghrib",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Maghrib", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Maghrib>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            maghrib.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            maghrib.SetSentinelFromProviderValue("00:00:00");

            var maghribCongregation = runtimeEntityType.AddProperty(
                "MaghribCongregation",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("MaghribCongregation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<MaghribCongregation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            maghribCongregation.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            maghribCongregation.SetSentinelFromProviderValue("00:00:00");

            var shuruq = runtimeEntityType.AddProperty(
                "Shuruq",
                typeof(LocalTime),
                propertyInfo: typeof(MawaqitPrayerTimes).GetProperty("Shuruq", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MawaqitPrayerTimes).GetField("<Shuruq>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            shuruq.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                keyComparer: new ValueComparer<LocalTime>(
                    (LocalTime v1, LocalTime v2) => v1.Equals(v2),
                    (LocalTime v) => v.GetHashCode(),
                    (LocalTime v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<LocalTime, string>(
                    (LocalTime x) => x.GetStringForDBColumn(),
                    (string x) => x.GetLocalTimeFromDBColumnString()),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<LocalTime, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<LocalTime, string>(
                        (LocalTime x) => x.GetStringForDBColumn(),
                        (string x) => x.GetLocalTimeFromDBColumnString())));
            shuruq.SetSentinelFromProviderValue("00:00:00");

            var key = runtimeEntityType.AddKey(
                new[] { iD });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "MawaqitPrayerTimes");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
