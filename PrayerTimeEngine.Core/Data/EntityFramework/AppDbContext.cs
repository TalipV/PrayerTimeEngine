using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.EntityFramework.Configurations;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Reflection;

namespace PrayerTimeEngine.Core.Data.EntityFramework
{
    public class AppDbContext(
            DbContextOptions options,
            AppDbContextMetaData appDbContextMetaData,
            ISystemInfoService systemInfoService
        ) : DbContext(options)
    {
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileLocationConfig> ProfileLocations { get; set; }
        public DbSet<ProfileTimeConfig> ProfileConfigs { get; set; }
        public DbSet<ProfilePlaceInfo> PlaceInfos { get; set; }
        public DbSet<TimezoneInfo> TimezoneInfos { get; set; }

        public DbSet<MuwaqqitPrayerTimes> MuwaqqitPrayerTimes { get; set; }

        public DbSet<FaziletCountry> FaziletCountries { get; set; }
        public DbSet<FaziletCity> FaziletCities { get; set; }
        public DbSet<FaziletPrayerTimes> FaziletPrayerTimes { get; set; }

        public DbSet<SemerkandCountry> SemerkandCountries { get; set; }
        public DbSet<SemerkandCity> SemerkandCities { get; set; }
        public DbSet<SemerkandPrayerTimes> SemerkandPrayerTimes { get; set; }

        public DbSet<MawaqitPrayerTimes> MawaqitPrayerTimes { get; set; }
        public DbSet<MyMosqPrayerTimes> MyMosqPrayerTimes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProfileTimeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileLocationConfigConfiguration());

            foreach (var type in appDbContextMetaData.GetDbSetPropertyTypes())
            {
                configureNodaTimeProperties(modelBuilder, type);
            }

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

            modelBuilder
                .Entity<ProfilePlaceInfo>()
                .HasOne(x => x.Profile)
                .WithOne(x => x.PlaceInfo)
                .HasForeignKey<ProfilePlaceInfo>(x => x.ProfileID);
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
                else if (item.PropertyType == typeof(LocalTime))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<LocalTime>(item.Name)
                        .HasConversion(
                            x => x.GetStringForDBColumn(),
                            x => x.GetLocalTimeFromDBColumnString()
                        );
                }
                else if (item.PropertyType == typeof(LocalTime?))
                {
                    modelBuilder
                        .Entity(type)
                        .Property<LocalTime?>(item.Name)
                        .HasConversion(
                            x => x != null ? x.Value.GetStringForDBColumn() : null,
                            x => x != null ? x.GetLocalTimeFromDBColumnString() : null
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

        public override int SaveChanges()
        {
            onBeforeSave();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            onBeforeSave();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void onBeforeSave()
        {
            setInsertInstant();
        }

        private void setInsertInstant()
        {
            List<IInsertedAt> insertedAtEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is IInsertedAt)
                .Select(x => x.Entity).OfType<IInsertedAt>()
                .ToList();

            if (insertedAtEntities.Count == 0)
            {
                return;
            }

            Instant currentInstant = systemInfoService.GetCurrentInstant();

            foreach (IInsertedAt entity in insertedAtEntities)
            {
                entity.InsertInstant = currentInstant;
            }
        }

        // https://stackoverflow.com/a/63381238/2924986
        public TEntity DetachedClone<TEntity>(TEntity entity) where TEntity : class
            => Entry(entity).CurrentValues.Clone().ToObject() as TEntity;
    }
}