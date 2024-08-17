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
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;

#pragma warning disable 219, 612, 618
#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels
{
    internal partial class SemerkandCountryEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities.SemerkandCountry",
                typeof(SemerkandCountry),
                baseEntityType);

            var iD = runtimeEntityType.AddProperty(
                "ID",
                typeof(int),
                propertyInfo: typeof(SemerkandCountry).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(SemerkandCountry).GetField("<ID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
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

            var insertInstant = runtimeEntityType.AddProperty(
                "InsertInstant",
                typeof(Instant?),
                propertyInfo: typeof(SemerkandCountry).GetProperty("InsertInstant", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(SemerkandCountry).GetField("<InsertInstant>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
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

            var name = runtimeEntityType.AddProperty(
                "Name",
                typeof(string),
                propertyInfo: typeof(SemerkandCountry).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(SemerkandCountry).GetField("<Name>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            name.TypeMapping = SqliteStringTypeMapping.Default;

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
            runtimeEntityType.AddAnnotation("Relational:TableName", "SemerkandCountries");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
