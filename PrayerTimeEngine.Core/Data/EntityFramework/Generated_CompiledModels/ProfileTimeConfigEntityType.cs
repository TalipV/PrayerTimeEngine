﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework.Configurations;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;


#pragma warning disable 219, 612, 618
#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework
{
    internal partial class ProfileTimeConfigEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PrayerTimeEngine.Core.Domain.ProfileManagement.Models.ProfileTimeConfig",
                typeof(ProfileTimeConfig),
                baseEntityType);

            var iD = runtimeEntityType.AddProperty(
                "ID",
                typeof(int),
                propertyInfo: typeof(ProfileTimeConfig).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProfileTimeConfig).GetField("<ID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
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

            var calculationConfiguration = runtimeEntityType.AddProperty(
                "CalculationConfiguration",
                typeof(GenericSettingConfiguration),
                propertyInfo: typeof(ProfileTimeConfig).GetProperty("CalculationConfiguration", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProfileTimeConfig).GetField("<CalculationConfiguration>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            calculationConfiguration.TypeMapping = SqliteStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<GenericSettingConfiguration>(
                    (GenericSettingConfiguration v1, GenericSettingConfiguration v2) => object.Equals(v1, v2),
                    (GenericSettingConfiguration v) => v.GetHashCode(),
                    (GenericSettingConfiguration v) => v),
                keyComparer: new ValueComparer<GenericSettingConfiguration>(
                    (GenericSettingConfiguration v1, GenericSettingConfiguration v2) => object.Equals(v1, v2),
                    (GenericSettingConfiguration v) => v.GetHashCode(),
                    (GenericSettingConfiguration v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                converter: new ValueConverter<GenericSettingConfiguration, string>(
                    (GenericSettingConfiguration x) => JsonSerializer.Serialize(x, ProfileTimeConfigConfiguration.JsonOptions),
                    (string x) => JsonSerializer.Deserialize<GenericSettingConfiguration>(x, ProfileTimeConfigConfiguration.JsonOptions)),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<GenericSettingConfiguration, string>(
                    JsonStringReaderWriter.Instance,
                    new ValueConverter<GenericSettingConfiguration, string>(
                        (GenericSettingConfiguration x) => JsonSerializer.Serialize(x, ProfileTimeConfigConfiguration.JsonOptions),
                        (string x) => JsonSerializer.Deserialize<GenericSettingConfiguration>(x, ProfileTimeConfigConfiguration.JsonOptions))));

            var profileID = runtimeEntityType.AddProperty(
                "ProfileID",
                typeof(int),
                propertyInfo: typeof(ProfileTimeConfig).GetProperty("ProfileID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProfileTimeConfig).GetField("<ProfileID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: 0);
            profileID.TypeMapping = IntTypeMapping.Default.Clone(
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

            var timeType = runtimeEntityType.AddProperty(
                "TimeType",
                typeof(ETimeType),
                propertyInfo: typeof(ProfileTimeConfig).GetProperty("TimeType", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProfileTimeConfig).GetField("<TimeType>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            timeType.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<ETimeType>(
                    (ETimeType v1, ETimeType v2) => object.Equals((object)v1, (object)v2),
                    (ETimeType v) => v.GetHashCode(),
                    (ETimeType v) => v),
                keyComparer: new ValueComparer<ETimeType>(
                    (ETimeType v1, ETimeType v2) => object.Equals((object)v1, (object)v2),
                    (ETimeType v) => v.GetHashCode(),
                    (ETimeType v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"),
                converter: new ValueConverter<ETimeType, int>(
                    (ETimeType value) => (int)value,
                    (int value) => (ETimeType)value),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<ETimeType, int>(
                    JsonInt32ReaderWriter.Instance,
                    new ValueConverter<ETimeType, int>(
                        (ETimeType value) => (int)value,
                        (int value) => (ETimeType)value)));
            timeType.SetSentinelFromProviderValue(0);

            var key = runtimeEntityType.AddKey(
                new[] { iD });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { profileID });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ProfileID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ID") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var profile = declaringEntityType.AddNavigation("Profile",
                runtimeForeignKey,
                onDependent: true,
                typeof(Profile),
                propertyInfo: typeof(ProfileTimeConfig).GetProperty("Profile", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProfileTimeConfig).GetField("<Profile>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var timeConfigs = principalEntityType.AddNavigation("TimeConfigs",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ProfileTimeConfig>),
                propertyInfo: typeof(Profile).GetProperty("TimeConfigs", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Profile).GetField("<TimeConfigs>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ProfileConfigs");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
