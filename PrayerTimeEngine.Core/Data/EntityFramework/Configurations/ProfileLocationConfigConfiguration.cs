﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Data.EntityFramework.Configurations
{
    public class ProfileLocationConfigConfiguration : IEntityTypeConfiguration<ProfileLocationConfig>
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {

        };

        public void Configure(EntityTypeBuilder<ProfileLocationConfig> builder)
        {
            builder
                .HasOne(x => x.Profile)
                .WithMany(x => x.LocationConfigs)
                .HasForeignKey(x => x.ProfileID);

            builder
                .Property(x => x.LocationData)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, JsonOptions),
                    x => JsonSerializer.Deserialize<BaseLocationData>(x, JsonOptions)
                );
        }
    }
}
