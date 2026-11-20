using System.Windows.Input;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Presentation.Localization;
using PrayerTimeEngine.Presentation.Pages.Main;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes;

public partial class DynamicPrayerTimeView : PrayerTimeCardsView
{
    private const string TimeFormat = "HH:mm:ss";
    private const string MissingTime = "xx:xx:xx";

    private readonly MainPageViewModel _mainPageViewModel;

    public DynamicPrayerTimeView(MainPageViewModel mainPageViewModel, ISystemInfoService systemInfoService)
        : base(systemInfoService)
    {
        _mainPageViewModel = mainPageViewModel;
        InitializeUI();
    }

    // Digits are the widest glyphs here, while the colons, the dash and the spaces of a time are
    // markedly narrower, so a plain character count would overestimate a time by roughly a third
    // and shrink the whole view for no reason.
    protected override double MainTimeEmWidth => 8.8;  // "00:00:00 - 00:00:00"
    protected override double SubTimeEmWidth => 4.0;   // "00:00:00"

    /// <summary>
    /// A card opens the settings of its section.
    /// </summary>
    protected override ICommand CardTapCommand => _mainPageViewModel.GoToSettingsPageCommand;

    protected override List<(ETimeSection Section, GenericPrayerTime Times)>? GetDisplayedPrayerTimes(IPrayerTimesDay prayerTimesSet)
        => (prayerTimesSet as DynamicPrayerTimesDaySet)?.CurrentDay?.AllPrayerTimes;

    protected override BlockDefinition[][] GetColumns() =>
    [
        [
            CreateBlock(AppResources.Prayer_Fajr, ETimeSection.Fajr, nameof(DynamicPrayerTimesDay.Fajr),
            [
                (AppResources.SubTime_Ghalas, nameof(FajrPrayerTime.Ghalas), nameof(DynamicPrayerTimeViewModel.ShowFajrGhalas)),
                (AppResources.SubTime_Redness, nameof(FajrPrayerTime.Karaha), nameof(DynamicPrayerTimeViewModel.ShowFajrRedness)),
            ]),
            CreateBlock(AppResources.Prayer_Dhuhr, ETimeSection.Dhuhr, nameof(DynamicPrayerTimesDay.Dhuhr), []),
            CreateBlock(AppResources.Prayer_Maghrib, ETimeSection.Maghrib, nameof(DynamicPrayerTimesDay.Maghrib),
            [
                (AppResources.SubTime_Sufficient, nameof(MaghribPrayerTime.SufficientTime), nameof(DynamicPrayerTimeViewModel.ShowMaghribSufficientTime)),
                (AppResources.SubTime_Ishtibak, nameof(MaghribPrayerTime.Ishtibak), nameof(DynamicPrayerTimeViewModel.ShowIshtibak)),
            ]),
        ],
        [
            CreateBlock(AppResources.Prayer_Duha, ETimeSection.Duha, nameof(DynamicPrayerTimesDay.Duha),
            [
                (AppResources.SubTime_Quarter, nameof(DuhaPrayerTime.QuarterOfDay), null),
                (AppResources.SubTime_Half, nameof(DuhaPrayerTime.HalfOfDay), null),
            ]),
            CreateBlock(AppResources.Prayer_Asr, ETimeSection.Asr, nameof(DynamicPrayerTimesDay.Asr),
            [
                (AppResources.SubTime_Mithlayn, nameof(AsrPrayerTime.Mithlayn), nameof(DynamicPrayerTimeViewModel.ShowMithlayn)),
                (AppResources.SubTime_Karaha, nameof(AsrPrayerTime.Karaha), nameof(DynamicPrayerTimeViewModel.ShowKaraha)),
            ]),
            CreateBlock(AppResources.Prayer_Isha, ETimeSection.Isha, nameof(DynamicPrayerTimesDay.Isha),
            [
                ("1/3", nameof(IshaPrayerTime.FirstThirdOfNight), null),
                ("1/2", nameof(IshaPrayerTime.MiddleOfNight), null),
                ("2/3", nameof(IshaPrayerTime.SecondThirdOfNight), null),
            ]),
        ],
    ];

    /// <summary>
    /// A single moment instead of a range, and not tied to one section of the day.
    /// </summary>
    protected override BlockDefinition GetMomentBlock()
        => new(
            AppResources.Prayer_Qibla,
            ETimeSection.General,
            CurrentDayBinding(nameof(DynamicPrayerTimesDay.Qibla)),
            value => FormatTime((value as GenericPrayerTime)?.Start) ?? MissingTime,
            []);

    private BlockDefinition CreateBlock(
        string name,
        ETimeSection section,
        string prayerTimeProperty,
        (string Name, string Property, string? ShowBinding)[] subTimes)
    {
        string binding = CurrentDayBinding(prayerTimeProperty);

        return new BlockDefinition(
            name,
            section,
            binding,
            value =>
            {
                var prayerTime = value as GenericPrayerTime;
                return $"{FormatTime(prayerTime?.Start) ?? MissingTime} - {FormatTime(prayerTime?.End) ?? MissingTime}";
            },
            [
                .. subTimes.Select(subTime => new SubTimeDefinition(
                    subTime.Name,
                    $"{binding}.{subTime.Property}",
                    value => FormatTime(value as ZonedDateTime?) ?? "",
                    subTime.ShowBinding))
            ]);
    }

    private string? FormatTime(ZonedDateTime? value)
        => SystemInfoService.GetInCurrentZone(value)?.ToString(TimeFormat, null);

    private static string CurrentDayBinding(string prayerTimeProperty)
        => $"{nameof(DynamicPrayerTimeViewModel.PrayerTimesSet)}.{nameof(DynamicPrayerTimesDaySet.CurrentDay)}.{prayerTimeProperty}";
}
