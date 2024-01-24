using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.EntityFramework.Configurations;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using System.Reflection;

namespace PrayerTimeEngine.Core.Data.EntityFramework
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileLocationConfig> ProfileLocations { get; set; }
        public DbSet<ProfileTimeConfig> ProfileConfigs { get; set; }

        public DbSet<MuwaqqitPrayerTimes> MuwaqqitPrayerTimes { get; set; }

        public DbSet<FaziletCountry> FaziletCountries { get; set; }
        public DbSet<FaziletCity> FaziletCities { get; set; }
        public DbSet<FaziletPrayerTimes> FaziletPrayerTimes { get; set; }

        public DbSet<SemerkandCountry> SemerkandCountries { get; set; }
        public DbSet<SemerkandCity> SemerkandCities { get; set; }
        public DbSet<SemerkandPrayerTimes> SemerkandPrayerTimes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProfileTimeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileLocationConfigConfiguration());

            configureNodaTimeProperties(modelBuilder, typeof(MuwaqqitPrayerTimes));
            configureNodaTimeProperties(modelBuilder, typeof(FaziletPrayerTimes));
            configureNodaTimeProperties(modelBuilder, typeof(SemerkandPrayerTimes));

            modelBuilder
                .Entity<FaziletCity>()
                .HasOne(x => x.Country)
                .WithMany(x => x.Cities)
                .HasForeignKey(x => x.CountryID);

            modelBuilder
                .Entity<SemerkandCity>()
                .HasOne(x => x.Country)
                .WithMany(x => x.Cities)
                .HasForeignKey(x => x.CountryID);
        }

        private static void configureNodaTimeProperties(ModelBuilder modelBuilder, Type type)
        {
            foreach (PropertyInfo item in type.GetProperties())
            {
                if (item.PropertyType == typeof(ZonedDateTime))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<ZonedDateTime>(item.Name)
                        .HasConversion(
                            x => x.GetStringForDBColumn(),
                            x => x.GetZonedDateTimeFromDBColumnString()
                        );
                }
                else if (item.PropertyType == typeof(ZonedDateTime?))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<ZonedDateTime?>(item.Name)
                        .HasConversion(
                            x => x != null ? x.Value.GetStringForDBColumn() : null,
                            x => x != null ? x.GetZonedDateTimeFromDBColumnString() : null
                        );
                }
                else if (item.PropertyType == typeof(LocalDate))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<LocalDate>(item.Name)
                        .HasConversion(
                            x => x.GetStringForDBColumn(),
                            x => x.GetLocalDateFromDBColumnString()
                        );
                }
                else if (item.PropertyType == typeof(LocalDate?))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<LocalDate?>(item.Name)
                        .HasConversion(
                            x => x != null ? x.Value.GetStringForDBColumn() : null,
                            x => x != null ? x.GetLocalDateFromDBColumnString() : null
                        );
                }
                else if (item.PropertyType == typeof(Instant))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<Instant>(item.Name)
                        .HasConversion(
                            x => x.GetStringForDBColumn(),
                            x => x.GetInstantFromDBColumnString()
                        );
                }
                else if (item.PropertyType == typeof(Instant?))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<Instant?>(item.Name)
                        .HasConversion(
                            x => x != null ? x.Value.GetStringForDBColumn() : null,
                            x => x != null ? x.GetInstantFromDBColumnString() : null
                        );
                }

            };
        }
    }
}