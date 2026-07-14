# Graph Report - PrayerTimeEngine  (2026-07-14)

## Corpus Check
- 288 files · ~93,640 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2246 nodes · 4834 edges · 158 communities (143 shown, 15 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 372 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4e393fd0`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- IFaziletRepository
- MainPageViewModel
- PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities
- .CalculatePrayerTimesAsync
- FaziletRepository
- BaseLocationData
- JsonConverter
- ConfigurationMapper
- BasicPlaceInfo
- SemerkandRepository
- SemerkandDynamicPrayerTimeProviderTests.cs
- BaseTest
- IPreferenceService
- .GetCitiesByCountryID
- MauiProgram.cs
- IProfileRepository
- .CreateCompleteTestDynamicProfile
- ETimeType
- DynamicPrayerTimesDay
- Profile
- .GetPrayerTimesAsync
- PrayerTimeEngine.Services.Notifications
- PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities
- EMosquePrayerTimeProviderType
- ISemerkandRepository
- MosquePrayerTimesDay
- PrayerTimeSummaryNotification
- MyMosqPrayerTimesDTO
- PrayerTimeEngine.MAUI
- PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels
- MyMosqApiService
- MuwaqqitRepository
- MyMosqRepository
- DynamicProfile
- ISystemInfoService
- PrayerTimeEngine.Core.Data.EntityFramework
- PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations
- .GetPrayerTimesAsync
- QiblahMapPage
- .GetPrayerTimesAsync
- MawaqitRepository
- PrayerTimeEngine.Core.Common.Attribute
- MainPage
- PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models
- PrayerTimeEngine.Core.Common
- AppDbContext
- NodaTimeExtensions
- MawaqitMosquePrayerTimeProviderBenchmark
- MuwaqqitDynamicPrayerTimeProviderBenchmark
- FaziletDynamicPrayerTimeProvider
- IMosqueDailyPrayerTimes
- MockHttpMessageHandler
- MyMosqMosqueDailyPrayerTimes
- MuwaqqitDynamicPrayerTimeProviderBenchmark.cs
- .GetPrayerTimesAsync
- .GetTimesByCityID
- SemerkandDynamicPrayerTimeProviderBenchmark
- IslamicDateCalculationService
- ProfileRepositoryTests
- MauiProgram
- IEntity
- PrayerTimeEngine.Core
- FaziletDynamicPrayerTimeProviderBenchmark
- DynamicPrayerTimesDaySet
- DynamicPrayerTimeViewModel
- MultiSelectPopup
- Configuration
- SettingsContentPage
- App
- PrayerTimeGraphicView
- BasePrayerTimeViewModel
- SemerkandDailyPrayerTimes
- PrayerTimeGraphicTimeVO
- PrayerTimeEngine.Core.Data.JsonSerialization
- PrayerTimeEngine.Core.Tests.Unit
- Program
- NavigationService
- .Initialize
- LocalDateConverter
- .Import
- IIslamicDateCalculationService
- AppDelegate
- DateTimeZoneConverter
- NullableLocalTimeConverter
- .Import_TwoDynamicProfilesAndOneMosqueProfile_ImportedProfilesAsExpected
- ResourceDictionary
- MethodTimeLogger
- AppDelegate
- .getCurrentLocation
- PrayerTimeEngine.Core.Tests.Common
- PrayerTimeEngine.Core.Tests.Unit
- FaziletCityEntityType
- ProfileLocationConfigEntityType
- ProfileTimeConfigEntityType
- SemerkandCityEntityType
- MuwaqqitDailyPrayerTimes
- MuwaqqitPrayerTimesResponseDTO
- GenericPrayerTime
- .getServiceProvider
- ProfilePlaceInfo
- CustomBaseViewModel
- QiblahMapPage.cs
- App
- PrayerTimeEngine
- AssertionConfigurations
- AppDbContextModel
- AppDbContextMetaData
- DynamicProfileEntityType
- IDailyPrayerTimes
- ProfileVersionStore
- DatabaseTablesPageViewModel
- .ReloadAsync
- ContentPage
- FaziletCountryEntityType
- FaziletDailyPrayerTimesEntityType
- MawaqitMosqueDailyPrayerTimesEntityType
- MawaqitPrayerTimesEntityType
- MosqueProfileEntityType
- MuwaqqitDailyPrayerTimesEntityType
- MyMosqMosqueDailyPrayerTimesEntityType
- MyMosqPrayerTimesEntityType
- ProfileEntityType
- SemerkandCountryEntityType
- SemerkandDailyPrayerTimesEntityType
- TimezoneInfoEntityType
- .DeleteCacheDataAsync
- LocalDateConverter
- ProfileDataTemplateSelector
- OffsetDateTimeConverter
- .createSectorOutline
- MosqueProfile
- LoggingLayout
- .LineBreakMode
- .CalculatePrayerTimesAsync
- SemerkandLocationData
- .GetPrayerTimesSet
- BenchmarkConfig.cs
- FaziletDailyPrayerTimes
- PrayerTimeEngine.Core.Tests.Integration
- DynamicPrayerTimeProviderManagerTests.cs
- PlaceServiceTests.cs
- MosquePrayerTimeProviderManagerTests.cs
- MosquePrayerTimeProviderFactoryTests.cs
- MosquePrayerTimeProviderManagerTests.cs
- MosquePrayerTimeProviderFactoryTests.cs
- DynamicPrayerTimeProviderFactoryTests.cs
- Colors.xaml
- .GetPrayerTimesAsync
- DynamicPrayerTimeView
- PlaceServiceTests.cs
- FaziletLocationData
- EntityValidationTests
- .Create
- CLAUDE.md

## God Nodes (most connected - your core abstractions)
1. `PrayerTimeEngine.Core.Common.Enum` - 50 edges
2. `PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models` - 50 edges
3. `PrayerTimeEngine.Core.Data.EntityFramework` - 49 edges
4. `ETimeType` - 45 edges
5. `AppDbContext` - 45 edges
6. `Profile` - 45 edges
7. `DynamicProfile` - 40 edges
8. `BaseTest` - 38 edges
9. `PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities` - 38 edges
10. `MainPageViewModel` - 36 edges

## Surprising Connections (you probably didn't know these)
- `FaziletDynamicPrayerTimeProviderBenchmark` --references--> `FaziletLocationData`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/FaziletDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Fazilet/Models/FaziletLocationData.cs
- `FaziletDynamicPrayerTimeProviderBenchmark` --references--> `FaziletDynamicPrayerTimeProvider`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/FaziletDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Fazilet/Services/FaziletDynamicPrayerTimeProvider.cs
- `MuwaqqitDynamicPrayerTimeProviderBenchmark` --references--> `MuwaqqitLocationData`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/MuwaqqitDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Muwaqqit/Models/MuwaqqitLocationData.cs
- `MuwaqqitDynamicPrayerTimeProviderBenchmark` --references--> `MuwaqqitDynamicPrayerTimeProvider`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/MuwaqqitDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Muwaqqit/Services/MuwaqqitDynamicPrayerTimeProvider.cs
- `SemerkandDynamicPrayerTimeProviderBenchmark` --references--> `SemerkandLocationData`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/SemerkandDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Semerkand/Models/SemerkandLocationData.cs

## Import Cycles
- None detected.

## Communities (158 total, 15 thin omitted)

### Community 0 - "IFaziletRepository"
Cohesion: 0.25
Nodes (9): CancellationToken, IEnumerable, List, LocalDate, Task, IFaziletRepository, Fact, Task (+1 more)

### Community 1 - "MainPageViewModel"
Cohesion: 0.05
Nodes (30): CancellationTokenSource, Debouncer, decimal, FilePickerFileType, ICommand, LogController, long, PickOptions (+22 more)

### Community 2 - "PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities"
Cohesion: 0.12
Nodes (13): PrayerTimeEngine.Core.Domain.PlaceManagement.Models, PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs, PrayerTimeEngine.Core.Domain.ConfigurationManagement, PrayerTimeEngine.Core.Tests.Integration.Domain.ConfigurationManagement, PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement, PrayerTimeEngine.Core.Domain.ProfileManagement.Services, PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces, PrayerTimeEngine.Core.Tests.Common.TestData (+5 more)

### Community 3 - ".CalculatePrayerTimesAsync"
Cohesion: 0.07
Nodes (38): CalculationErrors, ComplexCalculationResult, Exception, DynamicPrayerTimeProviderFactory, IDynamicPrayerTimeProviderFactory, CancellationToken, ConcurrentDictionary, EDynamicPrayerTimeProviderType (+30 more)

### Community 4 - "FaziletRepository"
Cohesion: 0.16
Nodes (12): CancellationToken, Func, IEnumerable, List, LocalDate, Task, ZonedDateTime, FaziletRepository (+4 more)

### Community 5 - "BaseLocationData"
Cohesion: 0.12
Nodes (17): CityID, CountryID, MatchedCityName, MatchedCountryName, EDynamicPrayerTimeProviderType, BaseLocationData, CancellationToken, Func (+9 more)

### Community 6 - "JsonConverter"
Cohesion: 0.24
Nodes (8): JsonConverter, JsonSerializerOptions, LocalTime, LocalTimePattern, Type, Utf8JsonReader, Utf8JsonWriter, LocalTimeConverter

### Community 7 - "ConfigurationMapper"
Cohesion: 0.17
Nodes (8): ConfigurationMapper, ICollection, ConfigurationDTO, ICollection, DynamicProfileConfigDTO, LocationConfigDTO, PlaceInfoDTO, TimeConfigDTO

### Community 8 - "BasicPlaceInfo"
Cohesion: 0.08
Nodes (28): PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs, CancellationToken, List, Task, IPlaceService, BasicPlaceInfo, LocationIQAddress, LocationIQPlace (+20 more)

### Community 9 - "SemerkandRepository"
Cohesion: 0.16
Nodes (12): CancellationToken, Func, IEnumerable, List, LocalDate, Task, ZonedDateTime, SemerkandRepository (+4 more)

### Community 10 - "SemerkandDynamicPrayerTimeProviderTests.cs"
Cohesion: 0.16
Nodes (7): PrayerTimeEngine.BenchmarkDotNet.Benchmarks, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Semerkand, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models, PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Semerkand, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes

### Community 11 - "BaseTest"
Cohesion: 0.09
Nodes (24): CallInfo, DbConnection, Guid, Action, IDbContextFactory, ServiceProvider, BaseTest, string (+16 more)

### Community 12 - "IPreferenceService"
Cohesion: 0.07
Nodes (15): PrayerTimeEngine.Services, IPreferenceService, CancellationToken, Task, AppInitializer, CancellationToken, Task, IAppInitializer (+7 more)

### Community 13 - ".GetCitiesByCountryID"
Cohesion: 0.11
Nodes (21): PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs, CancellationToken, Get, List, Task, IFaziletApiService, FaziletCityResponseDTO, List (+13 more)

### Community 14 - "MauiProgram.cs"
Cohesion: 0.11
Nodes (15): PrayerTimeEngine.Presentation.Services.SettingsContentPageFactory, PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler, PrayerTimeEngine.Presentation.Services, PrayerTimeEngine.Presentation.Views.MosquePrayerTimes, PrayerTimeEngine.Presentation.Pages.QiblahFinder, PrayerTimeEngine.Presentation.Pages.DatabaseTables, PrayerTimeEngine.Core.Domain.IslamicCalendar.Services, PrayerTimeEngine.Presentation (+7 more)

### Community 15 - "IProfileRepository"
Cohesion: 0.26
Nodes (7): CancellationToken, DynamicPrayerTimeProvider, ICollection, List, LocationData, Task, IProfileRepository

### Community 16 - ".CreateCompleteTestDynamicProfile"
Cohesion: 0.23
Nodes (8): EDynamicPrayerTimeProviderType, Fact, MemberData, Task, Theory, TheoryData, Trait, ProfileServiceTests

### Community 17 - "ETimeType"
Cohesion: 0.10
Nodes (14): ETimeType, EDynamicPrayerTimeProviderType, GenericSettingConfiguration, IView, ISettingConfigurationViewModel, IReadOnlyCollection, IView, StackLayout (+6 more)

### Community 18 - "DynamicPrayerTimesDay"
Cohesion: 0.09
Nodes (17): GenericPrayerTime, List, ZonedDateTime, DynamicPrayerTimesDay, ZonedDateTime, AsrPrayerTime, ZonedDateTime, DuhaPrayerTime (+9 more)

### Community 19 - "Profile"
Cohesion: 0.17
Nodes (13): IIncludableQueryable, IQueryable, CancellationToken, Task, Instant, Profile, CancellationToken, DynamicPrayerTimeProvider (+5 more)

### Community 20 - ".GetPrayerTimesAsync"
Cohesion: 0.14
Nodes (13): Benchmark, GlobalSetup, IDbContextFactory, LocalDate, SqliteConnection, string, MyMosqMosquePrayerTimeProviderBenchmark, AsyncKeyedLocker (+5 more)

### Community 21 - "PrayerTimeEngine.Services.Notifications"
Cohesion: 0.06
Nodes (24): androidPermission, BasePlatformPermission, BroadcastReceiver, Bundle, Context, PrayerTimeEngine.Platforms.Android.Permissions, PrayerTimeEngine.Platforms.Android, PrayerTimeEngine.Platforms.Android.Notifications (+16 more)

### Community 22 - "PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities"
Cohesion: 0.22
Nodes (7): PrayerTimeEngine.Core.Tests.Integration.Domain.MosquePrayerTimes.Providers.MyMosq, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces, PrayerTimeEngine.Core.Data.WebSocket.Interfaces, PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.MyMosq, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities, PrayerTimeEngine.Core.Data.WebSocket

### Community 23 - "EMosquePrayerTimeProviderType"
Cohesion: 0.15
Nodes (12): EMosquePrayerTimeProviderType, IMosquePrayerTimeProviderFactory, CancellationToken, EMosquePrayerTimeProviderType, Task, ZonedDateTime, MosquePrayerTimeProviderManager, MosquePrayerTimeProviderFactory (+4 more)

### Community 24 - "ISemerkandRepository"
Cohesion: 0.18
Nodes (14): CancellationToken, IEnumerable, List, LocalDate, Task, ISemerkandRepository, Instant, SemerkandCity (+6 more)

### Community 25 - "MosquePrayerTimesDay"
Cohesion: 0.12
Nodes (15): BasePrayerTimeViewModel, CancellationToken, EMosquePrayerTimeProviderType, Task, ZonedDateTime, IMosquePrayerTimeProviderManager, List, PrayerType (+7 more)

### Community 26 - "PrayerTimeSummaryNotification"
Cohesion: 0.15
Nodes (14): Builder, IBinder, CancellationToken, ILogger, int, Intent, LocalDate, string (+6 more)

### Community 27 - "MyMosqPrayerTimesDTO"
Cohesion: 0.17
Nodes (7): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.JsonConverters, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs, LocalDate, LocalTime, MyMosqPrayerTimesDTO, List, MyMosqResponseDTO

### Community 28 - "PrayerTimeEngine.MAUI"
Cohesion: 0.11
Nodes (19): CommunityToolkit.Maui (14.2.0), CommunityToolkit.Maui.Markup (7.0.1), Debounce.Core (1.0.0), Mapsui.Maui (5.1.0), MetroLog.Maui (2.1.0), Microsoft.Extensions.DependencyInjection (10.0.9), Microsoft.Extensions.Http (10.0.9), Microsoft.Extensions.Http.Resilience (10.7.0) (+11 more)

### Community 29 - "PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels"
Cohesion: 0.18
Nodes (3): PrayerTimeEngine.Core.Common.Extensions, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities, PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels

### Community 30 - "MyMosqApiService"
Cohesion: 0.07
Nodes (31): IAsyncEnumerable, IDisposable, ArraySegment, CancellationToken, Task, Uri, WebSocketMessageType, WebSocketReceiveResult (+23 more)

### Community 31 - "MuwaqqitRepository"
Cohesion: 0.16
Nodes (14): CancellationToken, Func, IEnumerable, List, LocalDate, Task, ZonedDateTime, MuwaqqitRepository (+6 more)

### Community 32 - "MyMosqRepository"
Cohesion: 0.20
Nodes (11): CancellationToken, Func, List, LocalDate, Task, ZonedDateTime, MyMosqRepository, Fact (+3 more)

### Community 33 - "DynamicProfile"
Cohesion: 0.20
Nodes (7): ICollection, DynamicProfile, CancellationToken, DateTimeZone, List, Task, ProfileService

### Community 34 - "ISystemInfoService"
Cohesion: 0.21
Nodes (6): CultureInfo, DateTimeZone, ZonedDateTime, ISystemInfoService, Grid, MosquePrayerTimeView

### Community 35 - "PrayerTimeEngine.Core.Data.EntityFramework"
Cohesion: 0.15
Nodes (9): PrayerTimeEngine.Core.Data.EntityFramework, PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Fazilet, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models, PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Fazilet, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities, PrayerTimeEngine.Core.Tests.Common (+1 more)

### Community 36 - "PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations"
Cohesion: 0.07
Nodes (17): PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations, Migration, ModelSnapshot, MigrationBuilder, ModelBuilder, InitialMigration, InitialMigration, MigrationBuilder (+9 more)

### Community 37 - ".GetPrayerTimesAsync"
Cohesion: 0.18
Nodes (11): CancellationToken, Get, Task, IMuwaqqitApiService, IMuwaqqitRepository, Fact, Task, MuwaqqitApiServiceTests (+3 more)

### Community 38 - "QiblahMapPage"
Cohesion: 0.17
Nodes (7): MapControl, MapEventArgs, MemoryLayer, int, QiblahMapPage, TileLayer, ViewportChangedEventArgs

### Community 39 - ".GetPrayerTimesAsync"
Cohesion: 0.21
Nodes (11): BaseCountryCityDynamicPrayerTimeProvider, AsyncKeyedLocker, AsyncNonKeyedLocker, CancellationToken, HashSet, int, List, Task (+3 more)

### Community 40 - "MawaqitRepository"
Cohesion: 0.18
Nodes (12): IPrayerTimeCacheCleaner, CancellationToken, Func, List, LocalDate, Task, ZonedDateTime, MawaqitRepository (+4 more)

### Community 41 - "PrayerTimeEngine.Core.Common.Attribute"
Cohesion: 0.11
Nodes (12): Attribute, PrayerTimeEngine.Core.Common.Attribute, ConfigurableSimpleTypeAttribute, DegreeTimeTypeAttribute, IsNotHidableTimeTypeAttribute, SimpleTimeTypeAttribute, TimeTypeForPrayerTypeAttribute, List (+4 more)

### Community 42 - "MainPage"
Cohesion: 0.14
Nodes (8): CarouselView, GraphicsView, IDispatcher, MainPageOptionsMenuService, Instant, Grid, Label, MainPage

### Community 43 - "PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models"
Cohesion: 0.11
Nodes (12): PrayerTimeEngine.Core.Domain.Models, PrayerTimeEngine.Presentation.Views.PrayerTimes, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Management, PrayerTimeEngine.Core.Domain.Models.PrayerTimes (+4 more)

### Community 44 - "PrayerTimeEngine.Core.Common"
Cohesion: 0.11
Nodes (12): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models, PrayerTimeEngine.Core.Tests.Integration.Domain.MosquePrayerTimes.Providers.Mawaqit, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers, PrayerTimeEngine.Core.Common (+4 more)

### Community 45 - "AppDbContext"
Cohesion: 0.18
Nodes (8): DbContext, DbSet, IDesignTimeDbContextFactory, JsonSerializerOptions, ModelBuilder, Type, AppDbContext, DesignTimeDbContextFactory

### Community 46 - "NodaTimeExtensions"
Cohesion: 0.13
Nodes (12): InstantPattern, LocalDateTimePattern, DateTimeZone, Instant, LocalDate, LocalDatePattern, LocalDateTime, LocalTime (+4 more)

### Community 47 - "MawaqitMosquePrayerTimeProviderBenchmark"
Cohesion: 0.13
Nodes (14): IMosquePrayerTimeProvider, GlobalSetup, IDbContextFactory, LocalDate, SqliteConnection, string, MawaqitMosquePrayerTimeProviderBenchmark, AsyncKeyedLocker (+6 more)

### Community 48 - "MuwaqqitDynamicPrayerTimeProviderBenchmark"
Cohesion: 0.23
Nodes (8): Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime, MuwaqqitDynamicPrayerTimeProviderBenchmark

### Community 49 - "FaziletDynamicPrayerTimeProvider"
Cohesion: 0.23
Nodes (10): AsyncKeyedLocker, AsyncNonKeyedLocker, CancellationToken, HashSet, IEnumerable, List, Task, TimeType (+2 more)

### Community 50 - "IMosqueDailyPrayerTimes"
Cohesion: 0.09
Nodes (23): Benchmark, Instant, LocalDate, LocalTime, IMosqueDailyPrayerTimes, CancellationToken, Task, IMawaqitApiService (+15 more)

### Community 51 - "MockHttpMessageHandler"
Cohesion: 0.07
Nodes (29): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.JsonConverters, HttpMessageHandler, HttpRequestMessage, HttpResponseMessage, JsonSerializerOptions, LocalTime, LocalTimePattern, Type (+21 more)

### Community 52 - "MyMosqMosqueDailyPrayerTimes"
Cohesion: 0.17
Nodes (12): CancellationToken, List, LocalDate, Task, IMyMosqRepository, Instant, LocalDate, LocalTime (+4 more)

### Community 53 - "MuwaqqitDynamicPrayerTimeProviderBenchmark.cs"
Cohesion: 0.21
Nodes (7): PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Muwaqqit, PrayerTimeEngine.Core.Domain, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Muwaqqit, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces

### Community 54 - ".GetPrayerTimesAsync"
Cohesion: 0.23
Nodes (9): IDynamicPrayerTimeProvider, AsyncKeyedLocker, CancellationToken, HashSet, List, Task, TimeType, ZonedDateTime (+1 more)

### Community 55 - ".GetTimesByCityID"
Cohesion: 0.31
Nodes (8): CancellationToken, Get, List, Task, ISemerkandApiService, Fact, Task, SemerkandApiServiceTests

### Community 56 - "SemerkandDynamicPrayerTimeProviderBenchmark"
Cohesion: 0.24
Nodes (8): Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime, SemerkandDynamicPrayerTimeProviderBenchmark

### Community 57 - "IslamicDateCalculationService"
Cohesion: 0.24
Nodes (9): int, IslamicDateCalculationService, LocalDate, MemberData, Theory, TheoryData, Trait, IslamicDateCalculationServiceTests (+1 more)

### Community 58 - "ProfileRepositoryTests"
Cohesion: 0.41
Nodes (5): Fact, IDbContextFactory, Task, Trait, ProfileRepositoryTests

### Community 59 - "MauiProgram"
Cohesion: 0.26
Nodes (7): DateTime, IServiceCollection, MauiAppBuilder, int, IServiceProvider, MauiApp, MauiProgram

### Community 60 - "IEntity"
Cohesion: 0.13
Nodes (11): Instant, IEntity, Instant, FaziletCity, ICollection, Instant, FaziletCountry, Instant (+3 more)

### Community 61 - "PrayerTimeEngine.Core"
Cohesion: 0.15
Nodes (13): AsyncAwaitBestPractices (10.0.0), AsyncKeyedLock (8.0.2), HtmlAgilityPack (1.12.4), Microsoft.EntityFrameworkCore.Design (10.0.9), Microsoft.EntityFrameworkCore.Sqlite (10.0.9), NodaTime (3.3.2), Refit.HttpClientFactory (13.1.0), SQLitePCLRaw.bundle_e_sqlite3 (3.0.3) (+5 more)

### Community 62 - "FaziletDynamicPrayerTimeProviderBenchmark"
Cohesion: 0.24
Nodes (8): Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime, FaziletDynamicPrayerTimeProviderBenchmark

### Community 63 - "DynamicPrayerTimesDaySet"
Cohesion: 0.12
Nodes (13): IDictionary, IReadOnlyList, EPrayerType, List, ZonedDateTime, DynamicPrayerTimesDaySet, List, PrayerType (+5 more)

### Community 64 - "DynamicPrayerTimeViewModel"
Cohesion: 0.33
Nodes (6): ESubTimeType, Instant, PrayerTimeGraphicSubTimeVO, Instant, List, DynamicPrayerTimeViewModel

### Community 65 - "MultiSelectPopup"
Cohesion: 0.20
Nodes (8): PrayerTimeEngine.Presentation.Popups, ObservableCollection, Popup, EventArgs, List, MultiSelectOption, MultiSelectPopup, VerticalStackLayout

### Community 66 - "Configuration"
Cohesion: 0.21
Nodes (7): ICollection, Configuration, JsonSerializerOptions, ConfigurationImportExportService, CancellationToken, Task, IConfigurationImportExportService

### Community 67 - "SettingsContentPage"
Cohesion: 0.20
Nodes (6): CheckBox, Picker, Grid, Label, StackLayout, SettingsContentPage

### Community 68 - "App"
Cohesion: 0.20
Nodes (7): IActivationState, appColors, Application, appStyles, App, Window, ResourceDictionary

### Community 69 - "PrayerTimeGraphicView"
Cohesion: 0.42
Nodes (6): ICanvas, IDrawable, Color, Instant, PrayerTimeGraphicView, RectF

### Community 70 - "BasePrayerTimeViewModel"
Cohesion: 0.27
Nodes (7): IPrayerTimeViewModel, CancellationToken, Instant, List, Task, ZonedDateTime, BasePrayerTimeViewModel

### Community 71 - "SemerkandDailyPrayerTimes"
Cohesion: 0.18
Nodes (11): DateTimeZone, LocalDate, LocalDateTime, LocalTime, SemerkandPrayerTimesResponseDTO, DateTimeZone, Instant, LocalDate (+3 more)

### Community 72 - "PrayerTimeGraphicTimeVO"
Cohesion: 0.18
Nodes (8): CancellationToken, Instant, Task, ZonedDateTime, IPrayerTimeViewModel, List, ZonedDateTime, PrayerTimeGraphicTimeVO

### Community 73 - "PrayerTimeEngine.Core.Data.JsonSerialization"
Cohesion: 0.20
Nodes (4): PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs, PrayerTimeEngine.Core.Data.JsonSerialization, SemerkandCityResponseDTO, SemerkandCountryResponseDTO

### Community 74 - "PrayerTimeEngine.Core.Tests.Unit"
Cohesion: 0.40
Nodes (3): PrayerTimeEngine.Core.Tests.Unit, Fact, WhereDoIPutThisStuffTests

### Community 75 - "Program"
Cohesion: 0.22
Nodes (5): MauiApplication, MauiApp, MainApplication, MauiApp, Program

### Community 76 - "NavigationService"
Cohesion: 0.29
Nodes (5): NavigationPage, Dictionary, Task, INavigationService, NavigationService

### Community 77 - ".Initialize"
Cohesion: 0.30
Nodes (5): AppDbContextModel, RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfilePlaceInfoEntityType

### Community 78 - "LocalDateConverter"
Cohesion: 0.31
Nodes (7): JsonSerializerOptions, LocalDate, LocalDatePattern, Type, Utf8JsonReader, Utf8JsonWriter, LocalDateConverter

### Community 79 - ".Import"
Cohesion: 0.36
Nodes (6): CancellationToken, Task, Fact, Task, Trait, ConfigurationImportExportServiceTests

### Community 81 - "AppDelegate"
Cohesion: 0.25
Nodes (4): PrayerTimeEngine.Platforms.iOS, MauiApp, AppDelegate, Program

### Community 82 - "DateTimeZoneConverter"
Cohesion: 0.31
Nodes (6): DateTimeZone, JsonSerializerOptions, Type, Utf8JsonReader, Utf8JsonWriter, DateTimeZoneConverter

### Community 83 - "NullableLocalTimeConverter"
Cohesion: 0.31
Nodes (7): JsonSerializerOptions, LocalTime, LocalTimePattern, Type, Utf8JsonReader, Utf8JsonWriter, NullableLocalTimeConverter

### Community 84 - ".Import_TwoDynamicProfilesAndOneMosqueProfile_ImportedProfilesAsExpected"
Cohesion: 0.39
Nodes (5): Fact, ServiceProvider, Task, Trait, ConfigurationImportExportServiceTests

### Community 85 - "ResourceDictionary"
Cohesion: 0.31
Nodes (8): CommonStates, Disabled, Normal, Off, On, ResourceDictionary, VisualState, VisualStateGroup

### Community 86 - "MethodTimeLogger"
Cohesion: 0.29
Nodes (5): ConcurrentBag, MethodBase, ILogger, MethodTimeLogger, TimeSpan

### Community 87 - "AppDelegate"
Cohesion: 0.22
Nodes (5): PrayerTimeEngine.Platforms.MacCatalyst, MauiUIApplicationDelegate, MauiApp, AppDelegate, Program

### Community 88 - ".getCurrentLocation"
Cohesion: 0.36
Nodes (3): MPoint, EventArgs, Task

### Community 89 - "PrayerTimeEngine.Core.Tests.Common"
Cohesion: 0.25
Nodes (8): AwesomeAssertions (9.4.0), coverlet.collector (10.0.1), xunit (2.9.3), PrayerTimeEngine.Core.Tests.Common, Microsoft.NET.Test.Sdk (18.7.0), NSubstitute (5.3.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

### Community 90 - "PrayerTimeEngine.Core.Tests.Unit"
Cohesion: 0.29
Nodes (7): BenchmarkDotNet (0.15.8), PrayerTimeEngine.BenchmarkDotNet, Microsoft.NET.Sdk, PrayerTimeEngine.Core.Tests.Unit, Microsoft.NET.Test.Sdk (18.7.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

### Community 91 - "FaziletCityEntityType"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, FaziletCityEntityType

### Community 92 - "ProfileLocationConfigEntityType"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfileLocationConfigEntityType

### Community 93 - "ProfileTimeConfigEntityType"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfileTimeConfigEntityType

### Community 94 - "SemerkandCityEntityType"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, SemerkandCityEntityType

### Community 95 - "MuwaqqitDailyPrayerTimes"
Cohesion: 0.18
Nodes (10): CancellationToken, IEnumerable, LocalDate, Task, DateTimeZone, Instant, LocalDate, LocalDateTime (+2 more)

### Community 96 - "MuwaqqitPrayerTimesResponseDTO"
Cohesion: 0.39
Nodes (5): DateTimeZone, LocalDate, LocalDateTime, OffsetDateTime, MuwaqqitPrayerTimesResponseDTO

### Community 97 - "GenericPrayerTime"
Cohesion: 0.25
Nodes (3): ZonedDateTime, GenericPrayerTime, MosquePrayerTime

### Community 98 - ".getServiceProvider"
Cohesion: 0.50
Nodes (4): Fact, ServiceProvider, Task, ProfileServiceTests

### Community 99 - "ProfilePlaceInfo"
Cohesion: 0.22
Nodes (4): Instant, ProfilePlaceInfo, Instant, TimezoneInfo

### Community 100 - "CustomBaseViewModel"
Cohesion: 0.25
Nodes (5): BaseViewModel, List, CustomBaseViewModel, SettingsHandlerPageViewModel, SettingsContentPageFactory

### Community 101 - "QiblahMapPage.cs"
Cohesion: 0.33
Nodes (3): PrayerTimeEngine.Extensions, Color, ColorExtensions

### Community 102 - "App"
Cohesion: 0.29
Nodes (4): PrayerTimeEngine.WinUI, MauiWinUIApplication, MauiApp, App

### Community 103 - "PrayerTimeEngine"
Cohesion: 0.25
Nodes (5): PrayerTimeEngine, string, AppApiKeys, string, AppConfig

### Community 105 - "AppDbContextModel"
Cohesion: 0.33
Nodes (4): IModel, bool, AppDbContextModel, RuntimeModel

### Community 106 - "AppDbContextMetaData"
Cohesion: 0.38
Nodes (4): Lazy, List, Type, AppDbContextMetaData

### Community 107 - "DynamicProfileEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, DynamicProfileEntityType

### Community 108 - "IDailyPrayerTimes"
Cohesion: 0.33
Nodes (5): DateTimeZone, LocalDate, LocalDateTime, ZonedDateTime, IDailyPrayerTimes

### Community 109 - "ProfileVersionStore"
Cohesion: 0.22
Nodes (3): IProfileVersionStore, ConcurrentDictionary, ProfileVersionStore

### Community 110 - "DatabaseTablesPageViewModel"
Cohesion: 0.33
Nodes (4): Action, Dictionary, List, DatabaseTablesPageViewModel

### Community 111 - ".ReloadAsync"
Cohesion: 0.33
Nodes (4): CollectionEntry, CancellationToken, Task, EFCoreExtensions

### Community 112 - "ContentPage"
Cohesion: 0.33
Nodes (5): ContentPage, DataGrid, HashSet, List, DatabaseTablesPage

### Community 113 - "FaziletCountryEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, FaziletCountryEntityType

### Community 114 - "FaziletDailyPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, FaziletDailyPrayerTimesEntityType

### Community 115 - "MawaqitMosqueDailyPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MawaqitMosqueDailyPrayerTimesEntityType

### Community 116 - "MawaqitPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MawaqitPrayerTimesEntityType

### Community 117 - "MosqueProfileEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MosqueProfileEntityType

### Community 118 - "MuwaqqitDailyPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MuwaqqitDailyPrayerTimesEntityType

### Community 119 - "MyMosqMosqueDailyPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MyMosqMosqueDailyPrayerTimesEntityType

### Community 120 - "MyMosqPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MyMosqPrayerTimesEntityType

### Community 121 - "ProfileEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, ProfileEntityType

### Community 122 - "SemerkandCountryEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, SemerkandCountryEntityType

### Community 123 - "SemerkandDailyPrayerTimesEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, SemerkandDailyPrayerTimesEntityType

### Community 124 - "TimezoneInfoEntityType"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, TimezoneInfoEntityType

### Community 125 - ".DeleteCacheDataAsync"
Cohesion: 0.33
Nodes (4): CancellationToken, Task, ZonedDateTime, IPrayerTimeCacheCleaner

### Community 126 - "LocalDateConverter"
Cohesion: 0.36
Nodes (6): JsonSerializerOptions, LocalDate, Type, Utf8JsonReader, Utf8JsonWriter, LocalDateConverter

### Community 127 - "ProfileDataTemplateSelector"
Cohesion: 0.50
Nodes (4): BindableObject, DataTemplate, DataTemplateSelector, ProfileDataTemplateSelector

### Community 128 - "OffsetDateTimeConverter"
Cohesion: 0.36
Nodes (6): JsonSerializerOptions, OffsetDateTime, Type, Utf8JsonReader, Utf8JsonWriter, OffsetDateTimeConverter

### Community 129 - ".createSectorOutline"
Cohesion: 0.40
Nodes (3): Coordinate, IFeature, List

### Community 130 - "MosqueProfile"
Cohesion: 0.29
Nodes (3): MosqueProfileConfigDTO, ProfileConfigDTO, MosqueProfile

### Community 131 - "LoggingLayout"
Cohesion: 0.40
Nodes (4): Layout, LogEventInfo, LogWriteContext, LoggingLayout

### Community 132 - ".LineBreakMode"
Cohesion: 0.40
Nodes (3): LineBreakMode, Label, MarkUpExtensions

### Community 133 - ".CalculatePrayerTimesAsync"
Cohesion: 0.40
Nodes (4): CancellationToken, Task, ZonedDateTime, IDynamicPrayerTimeProviderManager

### Community 135 - ".GetPrayerTimesSet"
Cohesion: 0.60
Nodes (3): CancellationToken, Task, ZonedDateTime

### Community 136 - "BenchmarkConfig.cs"
Cohesion: 0.50
Nodes (3): PrayerTimeEngine.BenchmarkDotNet, ManualConfig, BenchmarkConfig

### Community 137 - "FaziletDailyPrayerTimes"
Cohesion: 0.29
Nodes (6): DateTimeZone, Instant, LocalDate, LocalDateTime, ZonedDateTime, FaziletDailyPrayerTimes

### Community 138 - "PrayerTimeEngine.Core.Tests.Integration"
Cohesion: 0.50
Nodes (4): PrayerTimeEngine.Core.Tests.Integration, Microsoft.NET.Test.Sdk (18.7.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

### Community 151 - ".GetPrayerTimesAsync"
Cohesion: 0.38
Nodes (5): CancellationToken, List, LocalDate, Task, IMyMosqApiService

### Community 152 - "DynamicPrayerTimeView"
Cohesion: 0.60
Nodes (3): ContentView, Grid, DynamicPrayerTimeView

### Community 153 - "PlaceServiceTests.cs"
Cohesion: 0.50
Nodes (3): PrayerTimeEngine.Core.Domain.PlaceManagement.Services, PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ, PrayerTimeEngine.Core.Tests.Unit.Domain.PlaceManagement

## Knowledge Gaps
- **79 isolated node(s):** `PrayerTimeEngine.BenchmarkDotNet`, `BenchmarkDotNet (0.15.8)`, `Microsoft.NET.Sdk`, `coverlet.collector (10.0.1)`, `AwesomeAssertions (9.4.0)` (+74 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AppDbContext` connect `AppDbContext` to `DynamicProfile`, `MosqueProfile`, `ProfilePlaceInfo`, `SemerkandDailyPrayerTimes`, `FaziletDailyPrayerTimes`, `BaseTest`, `MawaqitMosquePrayerTimeProviderBenchmark`, `MuwaqqitDynamicPrayerTimeProviderBenchmark`, `IMosqueDailyPrayerTimes`, `Profile`, `.GetPrayerTimesAsync`, `MyMosqMosqueDailyPrayerTimes`, `SemerkandDynamicPrayerTimeProviderBenchmark`, `ISemerkandRepository`, `IEntity`, `PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels`, `FaziletDynamicPrayerTimeProviderBenchmark`, `MuwaqqitDailyPrayerTimes`?**
  _High betweenness centrality (0.134) - this node is a cross-community bridge._
- **Why does `BaseTest` connect `BaseTest` to `IFaziletRepository`, `.CalculatePrayerTimesAsync`, `FaziletRepository`, `BasicPlaceInfo`, `SemerkandRepository`, `.GetCitiesByCountryID`, `.CreateCompleteTestDynamicProfile`, `ISemerkandRepository`, `MyMosqApiService`, `MuwaqqitRepository`, `MyMosqRepository`, `PrayerTimeEngine.Core.Data.EntityFramework`, `.GetPrayerTimesAsync`, `MawaqitRepository`, `AppDbContext`, `MawaqitMosquePrayerTimeProviderBenchmark`, `IMosqueDailyPrayerTimes`, `MockHttpMessageHandler`, `MyMosqMosqueDailyPrayerTimes`, `.GetTimesByCityID`, `ProfileRepositoryTests`, `.Import`, `.Import_TwoDynamicProfilesAndOneMosqueProfile_ImportedProfilesAsExpected`, `.getServiceProvider`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **Why does `Profile` connect `Profile` to `MainPageViewModel`, `Configuration`, `.CalculatePrayerTimesAsync`, `DynamicProfile`, `MosqueProfile`, `PrayerTimeEngine.Core.Data.EntityFramework`, `BasePrayerTimeViewModel`, `AssertionConfigurations`, `PrayerTimeGraphicTimeVO`, `AppDbContext`, `IProfileRepository`, `.Create`, `PrayerTimeSummaryNotification`, `IEntity`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **What connects `PrayerTimeEngine.BenchmarkDotNet`, `BenchmarkDotNet (0.15.8)`, `Microsoft.NET.Sdk` to the rest of the system?**
  _79 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainPageViewModel` be split into smaller, more focused modules?**
  _Cohesion score 0.054203180785459264 - nodes in this community are weakly interconnected._
- **Should `PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities` be split into smaller, more focused modules?**
  _Cohesion score 0.1241565452091768 - nodes in this community are weakly interconnected._
- **Should `.CalculatePrayerTimesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.07272727272727272 - nodes in this community are weakly interconnected._