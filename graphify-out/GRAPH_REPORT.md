# Graph Report - C:\Users\talip\source\repos\PrayerTimeEngine  (2026-07-09)

## Corpus Check
- 307 files · ~87,875 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2194 nodes · 4773 edges · 151 communities (140 shown, 11 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 369 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31
- Community 32
- Community 33
- Community 34
- Community 35
- Community 36
- Community 37
- Community 38
- Community 39
- Community 40
- Community 41
- Community 42
- Community 43
- Community 44
- Community 45
- Community 46
- Community 47
- Community 48
- Community 49
- Community 50
- Community 51
- Community 52
- Community 53
- Community 54
- Community 55
- Community 56
- Community 57
- Community 58
- Community 59
- Community 60
- Community 61
- Community 62
- Community 63
- Community 64
- Community 65
- Community 66
- Community 67
- Community 68
- Community 69
- Community 70
- Community 71
- Community 72
- Community 73
- Community 74
- Community 75
- Community 76
- Community 77
- Community 78
- Community 79
- Community 80
- Community 81
- Community 82
- Community 83
- Community 84
- Community 85
- Community 86
- Community 87
- Community 88
- Community 89
- Community 90
- Community 91
- Community 92
- Community 93
- Community 94
- Community 95
- Community 96
- Community 97
- Community 98
- Community 99
- Community 100
- Community 101
- Community 102
- Community 103
- Community 104
- Community 105
- Community 106
- Community 107
- Community 108
- Community 109
- Community 110
- Community 111
- Community 112
- Community 113
- Community 114
- Community 115
- Community 116
- Community 117
- Community 118
- Community 119
- Community 120
- Community 121
- Community 122
- Community 123
- Community 124
- Community 125
- Community 126
- Community 127
- Community 128
- Community 129
- Community 130
- Community 131
- Community 132
- Community 133
- Community 134
- Community 135
- Community 136
- Community 137
- Community 138
- Community 139
- Community 140
- Community 141
- Community 142
- Community 143
- Community 144
- Community 145
- Community 146

## God Nodes (most connected - your core abstractions)
1. `PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models` - 50 edges
2. `PrayerTimeEngine.Core.Common.Enum` - 49 edges
3. `PrayerTimeEngine.Core.Data.EntityFramework` - 47 edges
4. `AppDbContext` - 45 edges
5. `Profile` - 45 edges
6. `ETimeType` - 44 edges
7. `DynamicProfile` - 40 edges
8. `BaseTest` - 38 edges
9. `PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities` - 38 edges
10. `MainPageViewModel` - 36 edges

## Surprising Connections (you probably didn't know these)
- `MuwaqqitDynamicPrayerTimeProviderBenchmark` --references--> `MuwaqqitLocationData`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/MuwaqqitDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Muwaqqit/Models/MuwaqqitLocationData.cs
- `MuwaqqitDynamicPrayerTimeProviderBenchmark` --references--> `MuwaqqitDynamicPrayerTimeProvider`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/MuwaqqitDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Muwaqqit/Services/MuwaqqitDynamicPrayerTimeProvider.cs
- `SemerkandDynamicPrayerTimeProviderBenchmark` --references--> `SemerkandLocationData`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/SemerkandDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Semerkand/Models/SemerkandLocationData.cs
- `SemerkandDynamicPrayerTimeProviderBenchmark` --references--> `SemerkandDynamicPrayerTimeProvider`  [EXTRACTED]
  PrayerTimeEngine.BenchmarkDotNet/Benchmarks/SemerkandDynamicPrayerTimeProviderBenchmark.cs → PrayerTimeEngine.Core/Domain/DynamicPrayerTimes/Providers/Semerkand/Services/SemerkandDynamicPrayerTimeProvider.cs
- `BaseTest` --references--> `AppDbContext`  [EXTRACTED]
  PrayerTimeEngine.Core.Tests.Common/BaseTest.cs → PrayerTimeEngine.Core/Data/EntityFramework/AppDbContext.cs

## Import Cycles
- None detected.

## Communities (151 total, 11 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (44): BaseCountryCityDynamicPrayerTimeProvider, Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime (+36 more)

### Community 1 - "Community 1"
Cohesion: 0.05
Nodes (30): CancellationTokenSource, Debouncer, decimal, FilePickerFileType, ICommand, LogController, long, PickOptions (+22 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (22): PrayerTimeEngine.Core.Domain.PlaceManagement.Models, PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs, PrayerTimeEngine.Core.Domain.ConfigurationManagement, PrayerTimeEngine.Core.Tests.Integration.Domain.ConfigurationManagement, PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement, PrayerTimeEngine.Core.Domain.ProfileManagement.Services, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models, PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces (+14 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (33): CalculationErrors, ComplexCalculationResult, Exception, DynamicPrayerTimeProviderFactory, IDynamicPrayerTimeProviderFactory, CancellationToken, ConcurrentDictionary, EDynamicPrayerTimeProviderType (+25 more)

### Community 4 - "Community 4"
Cohesion: 0.09
Nodes (23): IPrayerTimeCacheCleaner, CancellationToken, Func, IEnumerable, List, Task, ZonedDateTime, FaziletRepository (+15 more)

### Community 5 - "Community 5"
Cohesion: 0.07
Nodes (26): CityID, CountryID, MatchedCityName, MatchedCountryName, EDynamicPrayerTimeProviderType, BaseLocationData, CancellationToken, Func (+18 more)

### Community 6 - "Community 6"
Cohesion: 0.06
Nodes (34): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.JsonConverters, JsonConverter, DateTimeZone, JsonSerializerOptions, Type, Utf8JsonReader, Utf8JsonWriter, DateTimeZoneConverter (+26 more)

### Community 7 - "Community 7"
Cohesion: 0.07
Nodes (24): ICollection, Configuration, CancellationToken, JsonSerializerOptions, Task, ConfigurationImportExportService, ConfigurationMapper, ICollection (+16 more)

### Community 8 - "Community 8"
Cohesion: 0.09
Nodes (24): PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs, BasicPlaceInfo, LocationIQAddress, LocationIQPlace, LocationIQTimezone, LocationIQTimezoneResponseDTO, CancellationToken, Get (+16 more)

### Community 9 - "Community 9"
Cohesion: 0.14
Nodes (16): Instant, SemerkandCity, ICollection, Instant, SemerkandCountry, CancellationToken, Func, IEnumerable (+8 more)

### Community 10 - "Community 10"
Cohesion: 0.12
Nodes (16): PrayerTimeEngine.BenchmarkDotNet.Benchmarks, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services, PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Fazilet, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Semerkand, PrayerTimeEngine.Core.Tests.Common.TestData, PrayerTimeEngine.Core.Tests.Integration.Domain.MosquePrayerTimes.Providers.Mawaqit, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces (+8 more)

### Community 11 - "Community 11"
Cohesion: 0.10
Nodes (22): CallInfo, DbConnection, Guid, Action, IDbContextFactory, ServiceProvider, BaseTest, Fact (+14 more)

### Community 12 - "Community 12"
Cohesion: 0.07
Nodes (15): PrayerTimeEngine.Services, IPreferenceService, CancellationToken, Task, AppInitializer, CancellationToken, Task, IAppInitializer (+7 more)

### Community 13 - "Community 13"
Cohesion: 0.11
Nodes (21): PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs, CancellationToken, Get, List, Task, IFaziletApiService, FaziletCityResponseDTO, List (+13 more)

### Community 14 - "Community 14"
Cohesion: 0.13
Nodes (16): PrayerTimeEngine.Presentation.Services.SettingsContentPageFactory, PrayerTimeEngine.Core.Domain.PlaceManagement.Services, PrayerTimeEngine.Presentation.Views.PrayerTimes, PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler, PrayerTimeEngine.Presentation.Services, PrayerTimeEngine.Presentation.Views.MosquePrayerTimes, PrayerTimeEngine.Presentation.Pages.DatabaseTables, PrayerTimeEngine.Presentation.Services.Navigation (+8 more)

### Community 15 - "Community 15"
Cohesion: 0.13
Nodes (13): EMosquePrayerTimeProviderType, CancellationToken, DynamicPrayerTimeProvider, ICollection, List, LocationData, Task, IProfileRepository (+5 more)

### Community 16 - "Community 16"
Cohesion: 0.20
Nodes (10): ETimeType, EDynamicPrayerTimeProviderType, List, Fact, MemberData, Task, Theory, TheoryData (+2 more)

### Community 17 - "Community 17"
Cohesion: 0.09
Nodes (12): TimeConfigDTO, EDynamicPrayerTimeProviderType, GenericSettingConfiguration, Instant, ProfileTimeConfig, IView, ISettingConfigurationViewModel, IReadOnlyCollection (+4 more)

### Community 18 - "Community 18"
Cohesion: 0.09
Nodes (17): GenericPrayerTime, List, ZonedDateTime, DynamicPrayerTimesDay, ZonedDateTime, AsrPrayerTime, ZonedDateTime, DuhaPrayerTime (+9 more)

### Community 19 - "Community 19"
Cohesion: 0.20
Nodes (11): IIncludableQueryable, IQueryable, CancellationToken, Task, CancellationToken, DynamicPrayerTimeProvider, ICollection, List (+3 more)

### Community 20 - "Community 20"
Cohesion: 0.11
Nodes (16): IMosquePrayerTimeProvider, Benchmark, GlobalSetup, IDbContextFactory, LocalDate, SqliteConnection, string, MyMosqMosquePrayerTimeProviderBenchmark (+8 more)

### Community 21 - "Community 21"
Cohesion: 0.10
Nodes (15): androidPermission, BasePlatformPermission, PrayerTimeEngine.Platforms.Android.Permissions, PrayerTimeEngine.Services.Notifications, isRuntime, string, Task, PrayerTimeSummaryNotificationHandler (+7 more)

### Community 22 - "Community 22"
Cohesion: 0.16
Nodes (9): PrayerTimeEngine.Core.Tests.Integration.Domain.MosquePrayerTimes.Providers.MyMosq, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces, PrayerTimeEngine.Core.Data.WebSocket.Interfaces, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models, PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.MyMosq, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities (+1 more)

### Community 23 - "Community 23"
Cohesion: 0.12
Nodes (13): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers, DateTimeZone, IMosquePrayerTimeProviderFactory, CancellationToken, EMosquePrayerTimeProviderType, Task, ZonedDateTime, MosquePrayerTimeProviderManager (+5 more)

### Community 24 - "Community 24"
Cohesion: 0.24
Nodes (9): CancellationToken, IEnumerable, List, Task, ZonedDateTime, ISemerkandRepository, Fact, Task (+1 more)

### Community 25 - "Community 25"
Cohesion: 0.12
Nodes (15): BasePrayerTimeViewModel, CancellationToken, EMosquePrayerTimeProviderType, Task, ZonedDateTime, IMosquePrayerTimeProviderManager, List, PrayerType (+7 more)

### Community 26 - "Community 26"
Cohesion: 0.15
Nodes (14): Builder, IBinder, CancellationToken, ILogger, int, Intent, LocalDate, string (+6 more)

### Community 27 - "Community 27"
Cohesion: 0.13
Nodes (12): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.JsonConverters, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs, CancellationToken, List, LocalDate, Task, IMyMosqApiService, LocalDate (+4 more)

### Community 28 - "Community 28"
Cohesion: 0.11
Nodes (19): CommunityToolkit.Maui (14.2.0), CommunityToolkit.Maui.Markup (7.0.1), Debounce.Core (1.0.0), Mapsui.Maui (5.1.0), MetroLog.Maui (2.1.0), Microsoft.Extensions.DependencyInjection (10.0.9), Microsoft.Extensions.Http (10.0.9), Microsoft.Extensions.Http.Resilience (10.7.0) (+11 more)

### Community 29 - "Community 29"
Cohesion: 0.19
Nodes (3): PrayerTimeEngine.Core.Common.Extensions, PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels, AppDbContextModel

### Community 30 - "Community 30"
Cohesion: 0.23
Nodes (9): IAsyncEnumerable, CancellationToken, GeneratedRegex, List, LocalDate, Regex, string, Task (+1 more)

### Community 31 - "Community 31"
Cohesion: 0.20
Nodes (11): CancellationToken, Func, IEnumerable, List, Task, ZonedDateTime, MuwaqqitRepository, Fact (+3 more)

### Community 32 - "Community 32"
Cohesion: 0.20
Nodes (11): CancellationToken, Func, List, LocalDate, Task, ZonedDateTime, MyMosqRepository, Fact (+3 more)

### Community 33 - "Community 33"
Cohesion: 0.27
Nodes (5): CancellationToken, ConcurrentDictionary, DateTimeZone, Task, ProfileService

### Community 34 - "Community 34"
Cohesion: 0.19
Nodes (8): ContentView, CultureInfo, ZonedDateTime, ISystemInfoService, Grid, MosquePrayerTimeView, Grid, DynamicPrayerTimeView

### Community 35 - "Community 35"
Cohesion: 0.18
Nodes (3): PrayerTimeEngine.Core.Data.EntityFramework, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities

### Community 36 - "Community 36"
Cohesion: 0.13
Nodes (9): PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations, Migration, MigrationBuilder, ModelSnapshot, ModelBuilder, InitialMigration, InitialMigration, ModelBuilder (+1 more)

### Community 37 - "Community 37"
Cohesion: 0.15
Nodes (10): PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs, CancellationToken, Get, Task, IMuwaqqitApiService, Fact, Task, MuwaqqitApiServiceTests (+2 more)

### Community 38 - "Community 38"
Cohesion: 0.17
Nodes (7): MapControl, MapEventArgs, MemoryLayer, int, QiblahMapPage, TileLayer, ViewportChangedEventArgs

### Community 39 - "Community 39"
Cohesion: 0.25
Nodes (10): AsyncKeyedLocker, AsyncNonKeyedLocker, CancellationToken, HashSet, int, List, Task, TimeType (+2 more)

### Community 40 - "Community 40"
Cohesion: 0.18
Nodes (6): ICollection, DynamicProfile, bool, IReadOnlyCollection, List, SettingsContentPageViewModel

### Community 41 - "Community 41"
Cohesion: 0.18
Nodes (9): Attribute, PrayerTimeEngine.Core.Common.Attribute, ConfigurableSimpleTypeAttribute, DegreeTimeTypeAttribute, IsNotHidableTimeTypeAttribute, SimpleTimeTypeAttribute, TimeTypeForPrayerTypeAttribute, List (+1 more)

### Community 42 - "Community 42"
Cohesion: 0.14
Nodes (8): CarouselView, GraphicsView, IDispatcher, MainPageOptionsMenuService, Instant, Grid, Label, MainPage

### Community 43 - "Community 43"
Cohesion: 0.16
Nodes (4): PrayerTimeEngine.Core.Domain.Models, PrayerTimeEngine.Presentation, PrayerTimeEngine.Core.Domain.Models.PrayerTimes, PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs

### Community 44 - "Community 44"
Cohesion: 0.25
Nodes (5): PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services, PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.Mawaqit, PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities

### Community 45 - "Community 45"
Cohesion: 0.18
Nodes (8): DbContext, DbSet, IDesignTimeDbContextFactory, JsonSerializerOptions, ModelBuilder, Type, AppDbContext, DesignTimeDbContextFactory

### Community 46 - "Community 46"
Cohesion: 0.17
Nodes (9): InstantPattern, Instant, LocalDate, LocalDatePattern, LocalTime, LocalTimePattern, ZonedDateTime, NodaTimeExtensions (+1 more)

### Community 47 - "Community 47"
Cohesion: 0.19
Nodes (10): GlobalSetup, IDbContextFactory, LocalDate, SqliteConnection, string, MawaqitMosquePrayerTimeProviderBenchmark, AsyncKeyedLocker, int (+2 more)

### Community 48 - "Community 48"
Cohesion: 0.23
Nodes (8): Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime, MuwaqqitDynamicPrayerTimeProviderBenchmark

### Community 49 - "Community 49"
Cohesion: 0.19
Nodes (9): CancellationToken, Task, IMawaqitApiService, Dictionary, IEnumerable, List, LocalTime, LocalTimePattern (+1 more)

### Community 50 - "Community 50"
Cohesion: 0.17
Nodes (12): CancellationToken, List, LocalDate, Task, IMawaqitRepository, Instant, LocalDate, LocalTime (+4 more)

### Community 51 - "Community 51"
Cohesion: 0.26
Nodes (8): CancellationToken, GeneratedRegex, Regex, Task, MawaqitApiService, Fact, Task, MawaqitApiServiceTests

### Community 52 - "Community 52"
Cohesion: 0.17
Nodes (12): CancellationToken, List, LocalDate, Task, IMyMosqRepository, Instant, LocalDate, LocalTime (+4 more)

### Community 53 - "Community 53"
Cohesion: 0.29
Nodes (6): PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Muwaqqit, PrayerTimeEngine.Core.Domain, PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Muwaqqit, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services, PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces

### Community 54 - "Community 54"
Cohesion: 0.23
Nodes (9): IDynamicPrayerTimeProvider, AsyncKeyedLocker, CancellationToken, HashSet, List, Task, TimeType, ZonedDateTime (+1 more)

### Community 55 - "Community 55"
Cohesion: 0.31
Nodes (8): Post, CancellationToken, List, Task, ISemerkandApiService, Fact, Task, SemerkandApiServiceTests

### Community 56 - "Community 56"
Cohesion: 0.24
Nodes (8): Benchmark, GlobalSetup, IDbContextFactory, List, SqliteConnection, TimeType, ZonedDateTime, SemerkandDynamicPrayerTimeProviderBenchmark

### Community 57 - "Community 57"
Cohesion: 0.24
Nodes (9): int, IslamicDateCalculationService, LocalDate, MemberData, Theory, TheoryData, Trait, IslamicDateCalculationServiceTests (+1 more)

### Community 58 - "Community 58"
Cohesion: 0.41
Nodes (5): Fact, IDbContextFactory, Task, Trait, ProfileRepositoryTests

### Community 59 - "Community 59"
Cohesion: 0.26
Nodes (7): DateTime, IServiceCollection, MauiAppBuilder, int, IServiceProvider, MauiApp, MauiProgram

### Community 60 - "Community 60"
Cohesion: 0.22
Nodes (9): IDisposable, ArraySegment, CancellationToken, Task, Uri, WebSocketMessageType, WebSocketReceiveResult, WebSocketState (+1 more)

### Community 61 - "Community 61"
Cohesion: 0.15
Nodes (13): AsyncAwaitBestPractices (10.0.0), AsyncKeyedLock (8.0.2), HtmlAgilityPack (1.12.4), Microsoft.EntityFrameworkCore.Design (10.0.9), Microsoft.EntityFrameworkCore.Sqlite (10.0.9), NodaTime (3.3.2), Refit.HttpClientFactory (13.1.0), SQLitePCLRaw.bundle_e_sqlite3 (3.0.3) (+5 more)

### Community 62 - "Community 62"
Cohesion: 0.21
Nodes (8): Benchmark, Instant, LocalDate, LocalTime, IMosqueDailyPrayerTimes, CancellationToken, LocalDate, Task

### Community 63 - "Community 63"
Cohesion: 0.17
Nodes (9): EPrayerType, List, ZonedDateTime, DynamicPrayerTimesDaySet, List, PrayerType, Times, ZonedDateTime (+1 more)

### Community 64 - "Community 64"
Cohesion: 0.42
Nodes (6): ESubTimeType, Instant, PrayerTimeGraphicSubTimeVO, Instant, List, DynamicPrayerTimeViewModel

### Community 65 - "Community 65"
Cohesion: 0.20
Nodes (8): PrayerTimeEngine.Presentation.Popups, ObservableCollection, Popup, EventArgs, List, MultiSelectOption, MultiSelectPopup, VerticalStackLayout

### Community 66 - "Community 66"
Cohesion: 0.24
Nodes (8): ArraySegment, CancellationToken, Task, Uri, WebSocketMessageType, WebSocketReceiveResult, WebSocketState, WebSocketClient

### Community 67 - "Community 67"
Cohesion: 0.20
Nodes (6): CheckBox, Picker, Grid, Label, StackLayout, SettingsContentPage

### Community 68 - "Community 68"
Cohesion: 0.20
Nodes (7): IActivationState, appColors, Application, appStyles, App, Window, ResourceDictionary

### Community 69 - "Community 69"
Cohesion: 0.42
Nodes (6): ICanvas, IDrawable, Color, Instant, PrayerTimeGraphicView, RectF

### Community 70 - "Community 70"
Cohesion: 0.27
Nodes (7): IPrayerTimeViewModel, CancellationToken, Instant, List, Task, ZonedDateTime, BasePrayerTimeViewModel

### Community 71 - "Community 71"
Cohesion: 0.27
Nodes (8): DateTimeZone, LocalDate, LocalTime, ZonedDateTime, SemerkandPrayerTimesResponseDTO, Instant, ZonedDateTime, SemerkandDailyPrayerTimes

### Community 72 - "Community 72"
Cohesion: 0.18
Nodes (8): CancellationToken, Instant, Task, ZonedDateTime, IPrayerTimeViewModel, List, ZonedDateTime, PrayerTimeGraphicTimeVO

### Community 73 - "Community 73"
Cohesion: 0.20
Nodes (4): PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs, PrayerTimeEngine.Core.Data.JsonSerialization, SemerkandCityResponseDTO, SemerkandCountryResponseDTO

### Community 74 - "Community 74"
Cohesion: 0.22
Nodes (5): PrayerTimeEngine.Core.Tests.Unit, Fact, EntityValidationTests, Fact, WhereDoIPutThisStuffTests

### Community 75 - "Community 75"
Cohesion: 0.20
Nodes (5): MauiApplication, MauiApp, MainApplication, MauiApp, Program

### Community 76 - "Community 76"
Cohesion: 0.29
Nodes (5): NavigationPage, Dictionary, Task, INavigationService, NavigationService

### Community 77 - "Community 77"
Cohesion: 0.40
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfilePlaceInfoEntityType

### Community 78 - "Community 78"
Cohesion: 0.27
Nodes (7): JsonSerializerOptions, LocalDate, LocalDatePattern, Type, Utf8JsonReader, Utf8JsonWriter, LocalDateConverter

### Community 79 - "Community 79"
Cohesion: 0.28
Nodes (5): Bundle, PrayerTimeEngine.Platforms.Android, PrayerTimeEngine.Platforms.Android.Notifications, MauiAppCompatActivity, MainActivity

### Community 80 - "Community 80"
Cohesion: 0.22
Nodes (4): PrayerTimeEngine.Core.Domain.IslamicCalendar.Services, PrayerTimeEngine.Core.Domain.IslamicCalendar.Interfaces, PrayerTimeEngine.Core.Tests.Unit.Domain.IslamicCalendar, IIslamicDateCalculationService

### Community 81 - "Community 81"
Cohesion: 0.22
Nodes (5): PrayerTimeEngine.Platforms.iOS, MauiUIApplicationDelegate, MauiApp, AppDelegate, Program

### Community 82 - "Community 82"
Cohesion: 0.28
Nodes (7): HttpMessageHandler, HttpRequestMessage, HttpResponseMessage, CancellationToken, Func, Task, MockHttpMessageHandler

### Community 83 - "Community 83"
Cohesion: 0.31
Nodes (7): JsonSerializerOptions, LocalTime, LocalTimePattern, Type, Utf8JsonReader, Utf8JsonWriter, NullableLocalTimeConverter

### Community 84 - "Community 84"
Cohesion: 0.39
Nodes (5): Fact, ServiceProvider, Task, Trait, ConfigurationImportExportServiceTests

### Community 85 - "Community 85"
Cohesion: 0.31
Nodes (8): CommonStates, Disabled, Normal, Off, On, ResourceDictionary, VisualState, VisualStateGroup

### Community 86 - "Community 86"
Cohesion: 0.29
Nodes (5): ConcurrentBag, MethodBase, ILogger, MethodTimeLogger, TimeSpan

### Community 87 - "Community 87"
Cohesion: 0.25
Nodes (4): PrayerTimeEngine.Platforms.MacCatalyst, MauiApp, AppDelegate, Program

### Community 88 - "Community 88"
Cohesion: 0.36
Nodes (3): MPoint, EventArgs, Task

### Community 89 - "Community 89"
Cohesion: 0.25
Nodes (8): AwesomeAssertions (9.4.0), coverlet.collector (10.0.1), xunit (2.9.3), PrayerTimeEngine.Core.Tests.Common, Microsoft.NET.Test.Sdk (18.7.0), NSubstitute (5.3.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

### Community 90 - "Community 90"
Cohesion: 0.29
Nodes (7): BenchmarkDotNet (0.15.8), PrayerTimeEngine.BenchmarkDotNet, Microsoft.NET.Sdk, PrayerTimeEngine.Core.Tests.Unit, Microsoft.NET.Test.Sdk (18.7.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

### Community 91 - "Community 91"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, FaziletCityEntityType

### Community 92 - "Community 92"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfileLocationConfigEntityType

### Community 93 - "Community 93"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, ProfileTimeConfigEntityType

### Community 94 - "Community 94"
Cohesion: 0.39
Nodes (4): RuntimeEntityType, RuntimeForeignKey, RuntimeModel, SemerkandCityEntityType

### Community 95 - "Community 95"
Cohesion: 0.32
Nodes (6): CancellationToken, IEnumerable, Task, ZonedDateTime, IMuwaqqitRepository, MuwaqqitDynamicPrayerTimeProviderTests

### Community 96 - "Community 96"
Cohesion: 0.39
Nodes (5): DateTimeZone, LocalDate, OffsetDateTime, ZonedDateTime, MuwaqqitPrayerTimesResponseDTO

### Community 97 - "Community 97"
Cohesion: 0.25
Nodes (3): ZonedDateTime, GenericPrayerTime, MosquePrayerTime

### Community 98 - "Community 98"
Cohesion: 0.50
Nodes (4): Fact, ServiceProvider, Task, ProfileServiceTests

### Community 99 - "Community 99"
Cohesion: 0.54
Nodes (3): Fact, Task, MyMosqApiServiceTests

### Community 100 - "Community 100"
Cohesion: 0.29
Nodes (4): BaseViewModel, List, CustomBaseViewModel, SettingsHandlerPageViewModel

### Community 101 - "Community 101"
Cohesion: 0.29
Nodes (4): PrayerTimeEngine.Extensions, PrayerTimeEngine.Presentation.Pages.QiblahFinder, Color, ColorExtensions

### Community 102 - "Community 102"
Cohesion: 0.29
Nodes (4): PrayerTimeEngine.WinUI, MauiWinUIApplication, MauiApp, App

### Community 103 - "Community 103"
Cohesion: 0.29
Nodes (5): PrayerTimeEngine, string, AppApiKeys, string, AppConfig

### Community 105 - "Community 105"
Cohesion: 0.33
Nodes (4): IModel, bool, AppDbContextModel, RuntimeModel

### Community 106 - "Community 106"
Cohesion: 0.38
Nodes (4): Lazy, List, Type, AppDbContextMetaData

### Community 107 - "Community 107"
Cohesion: 0.43
Nodes (3): RuntimeEntityType, RuntimeModel, DynamicProfileEntityType

### Community 108 - "Community 108"
Cohesion: 0.33
Nodes (5): Instant, ZonedDateTime, MuwaqqitDailyPrayerTimes, ZonedDateTime, IDailyPrayerTimes

### Community 109 - "Community 109"
Cohesion: 0.29
Nodes (3): EDynamicPrayerTimeProviderType, MuwaqqitCalculationConfiguration, MuwaqqitDegreeCalculationConfiguration

### Community 110 - "Community 110"
Cohesion: 0.33
Nodes (4): Action, Dictionary, List, DatabaseTablesPageViewModel

### Community 111 - "Community 111"
Cohesion: 0.33
Nodes (4): CollectionEntry, CancellationToken, Task, EFCoreExtensions

### Community 112 - "Community 112"
Cohesion: 0.33
Nodes (5): ContentPage, DataGrid, HashSet, List, DatabaseTablesPage

### Community 113 - "Community 113"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, FaziletCountryEntityType

### Community 114 - "Community 114"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, FaziletDailyPrayerTimesEntityType

### Community 115 - "Community 115"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MawaqitMosqueDailyPrayerTimesEntityType

### Community 116 - "Community 116"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MawaqitPrayerTimesEntityType

### Community 117 - "Community 117"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MosqueProfileEntityType

### Community 118 - "Community 118"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MuwaqqitDailyPrayerTimesEntityType

### Community 119 - "Community 119"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MyMosqMosqueDailyPrayerTimesEntityType

### Community 120 - "Community 120"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, MyMosqPrayerTimesEntityType

### Community 121 - "Community 121"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, ProfileEntityType

### Community 122 - "Community 122"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, SemerkandCountryEntityType

### Community 123 - "Community 123"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, SemerkandDailyPrayerTimesEntityType

### Community 124 - "Community 124"
Cohesion: 0.53
Nodes (3): RuntimeEntityType, RuntimeModel, TimezoneInfoEntityType

### Community 125 - "Community 125"
Cohesion: 0.33
Nodes (4): CancellationToken, Task, ZonedDateTime, IPrayerTimeCacheCleaner

### Community 126 - "Community 126"
Cohesion: 0.53
Nodes (3): Fact, Task, SemerkandDynamicPrayerTimeProviderTests

### Community 127 - "Community 127"
Cohesion: 0.50
Nodes (4): BindableObject, DataTemplate, DataTemplateSelector, ProfileDataTemplateSelector

### Community 128 - "Community 128"
Cohesion: 0.40
Nodes (4): BroadcastReceiver, Context, Intent, BootBroadcastReceiver

### Community 129 - "Community 129"
Cohesion: 0.40
Nodes (3): Coordinate, IFeature, List

### Community 130 - "Community 130"
Cohesion: 0.40
Nodes (4): IDictionary, IReadOnlyList, List, TimeTypeAttributeService

### Community 131 - "Community 131"
Cohesion: 0.40
Nodes (4): Layout, LogEventInfo, LogWriteContext, LoggingLayout

### Community 132 - "Community 132"
Cohesion: 0.40
Nodes (3): LineBreakMode, Label, MarkUpExtensions

### Community 133 - "Community 133"
Cohesion: 0.40
Nodes (4): CancellationToken, Task, ZonedDateTime, IDynamicPrayerTimeProviderManager

### Community 135 - "Community 135"
Cohesion: 0.60
Nodes (3): CancellationToken, Task, ZonedDateTime

### Community 136 - "Community 136"
Cohesion: 0.50
Nodes (3): PrayerTimeEngine.BenchmarkDotNet, ManualConfig, BenchmarkConfig

### Community 138 - "Community 138"
Cohesion: 0.50
Nodes (4): PrayerTimeEngine.Core.Tests.Integration, Microsoft.NET.Test.Sdk (18.7.0), xunit.runner.visualstudio (3.1.5), Microsoft.NET.Sdk

## Knowledge Gaps
- **78 isolated node(s):** `PrayerTimeEngine.BenchmarkDotNet`, `BenchmarkDotNet (0.15.8)`, `Microsoft.NET.Sdk`, `coverlet.collector (10.0.1)`, `AwesomeAssertions (9.4.0)` (+73 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **11 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `BaseTest` connect `Community 11` to `Community 0`, `Community 3`, `Community 4`, `Community 7`, `Community 8`, `Community 9`, `Community 13`, `Community 16`, `Community 24`, `Community 31`, `Community 32`, `Community 35`, `Community 37`, `Community 45`, `Community 50`, `Community 51`, `Community 52`, `Community 55`, `Community 58`, `Community 60`, `Community 84`, `Community 95`, `Community 98`, `Community 99`, `Community 126`?**
  _High betweenness centrality (0.153) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `Community 45` to `Community 0`, `Community 35`, `Community 5`, `Community 71`, `Community 40`, `Community 9`, `Community 7`, `Community 11`, `Community 108`, `Community 47`, `Community 48`, `Community 15`, `Community 50`, `Community 19`, `Community 20`, `Community 52`, `Community 17`, `Community 56`?**
  _High betweenness centrality (0.141) - this node is a cross-community bridge._
- **Why does `PrayerTimeEngine.Core.Data.EntityFramework` connect `Community 35` to `Community 0`, `Community 2`, `Community 36`, `Community 5`, `Community 10`, `Community 106`, `Community 44`, `Community 43`, `Community 74`, `Community 111`, `Community 15`, `Community 14`, `Community 12`, `Community 53`, `Community 22`, `Community 29`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **What connects `PrayerTimeEngine.BenchmarkDotNet`, `BenchmarkDotNet (0.15.8)`, `Microsoft.NET.Sdk` to the rest of the system?**
  _78 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.05664556962025316 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.054203180785459264 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.08367254635911352 - nodes in this community are weakly interconnected._