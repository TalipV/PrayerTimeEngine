using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Reflection;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Data.EntityFramework;

public class AppDbContext(
        DbContextOptions options,
        AppDbContextMetaData appDbContextMetaData,
        ISystemInfoService systemInfoService
    ) : DbContext(options)
{
    public static readonly JsonSerializerOptions SerializerOptions = new();

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<DynamicProfile> DynamicProfiles { get; set; }
    public DbSet<MosqueProfile> MosqueProfiles { get; set; }
    public DbSet<ProfileLocationConfig> ProfileLocations { get; set; }
    public DbSet<ProfileTimeConfig> ProfileConfigs { get; set; }
    public DbSet<ProfilePlaceInfo> PlaceInfos { get; set; }
    public DbSet<TimezoneInfo> TimezoneInfos { get; set; }

    public DbSet<MuwaqqitDailyPrayerTimes> MuwaqqitPrayerTimes { get; set; }

    public DbSet<FaziletCountry> FaziletCountries { get; set; }
    public DbSet<FaziletCity> FaziletCities { get; set; }
    public DbSet<FaziletDailyPrayerTimes> FaziletPrayerTimes { get; set; }

    public DbSet<SemerkandCountry> SemerkandCountries { get; set; }
    public DbSet<SemerkandCity> SemerkandCities { get; set; }
    public DbSet<SemerkandDailyPrayerTimes> SemerkandPrayerTimes { get; set; }

    public DbSet<MawaqitMosqueDailyPrayerTimes> MawaqitPrayerTimes { get; set; }
    public DbSet<MyMosqMosqueDailyPrayerTimes> MyMosqPrayerTimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProfileTimeConfig>()
            .HasOne(x => x.Profile)
            .WithMany(x => x.TimeConfigs)
            .HasForeignKey(x => x.ProfileID);

        modelBuilder
            .Entity<ProfileTimeConfig>()
            .Property(x => x.CalculationConfiguration)
            .HasConversion(
                x => JsonSerializer.Serialize(x, SerializerOptions),
                x => JsonSerializer.Deserialize<GenericSettingConfiguration>(x, SerializerOptions)
            );

        modelBuilder
            .Entity<ProfileLocationConfig>()
            .HasOne(x => x.Profile)
            .WithMany(x => x.LocationConfigs)
            .HasForeignKey(x => x.ProfileID);

        modelBuilder
            .Entity<ProfileLocationConfig>()
            .Property(x => x.LocationData)
            .HasConversion(
                x => JsonSerializer.Serialize(x, SerializerOptions),
                x => JsonSerializer.Deserialize<BaseLocationData>(x, SerializerOptions)
            );

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
        List<IEntity> insertedAtEntities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is IEntity)
            .Select(x => x.Entity).OfType<IEntity>()
            .ToList();

        if (insertedAtEntities.Count == 0)
        {
            return;
        }

        Instant currentInstant = systemInfoService.GetCurrentInstant();

        foreach (IEntity entity in insertedAtEntities)
        {
            entity.InsertInstant = currentInstant;
        }
    }

    // https://stackoverflow.com/a/63381238/2924986
    public TEntity DetachedClone<TEntity>(TEntity entity) where TEntity : class
        => Entry(entity).CurrentValues.Clone().ToObject() as TEntity;
}