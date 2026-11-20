using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;

public partial class MosquePrayerTimeView : PrayerTimeCardsView
{
    // mosques publish their times to the minute
    private const string TimeFormat = "HH:mm";
    private const string MissingTime = "xx:xx";

    public MosquePrayerTimeView(ISystemInfoService systemInfoService)
        : base(systemInfoService)
    {
        InitializeUI();
    }

    protected override double MainTimeEmWidth => 5.8;  // "00:00 - 00:00"
    protected override double SubTimeEmWidth => 2.6;   // "00:00"

    protected override List<(ETimeSection Section, GenericPrayerTime Times)> GetDisplayedPrayerTimes(IPrayerTimesDay prayerTimesSet)
        => prayerTimesSet.AllPrayerTimes;

    /// <summary>
    /// Same order as the other profiles, with Jumu'ah in the place of Duha. The second Friday
    /// prayer belongs to Jumu'ah, so it sits on the line of the first one instead of a card of its own.
    /// </summary>
    protected override BlockDefinition[][] GetColumns() =>
    [
        [
            CreatePrayerBlock(AppResources.Prayer_Fajr, ETimeSection.Fajr, nameof(MosquePrayerTimesDay.Fajr), showEnd: true),
            CreatePrayerBlock(AppResources.Prayer_Dhuhr, ETimeSection.Dhuhr, nameof(MosquePrayerTimesDay.Dhuhr)),
            CreatePrayerBlock(AppResources.Prayer_Maghrib, ETimeSection.Maghrib, nameof(MosquePrayerTimesDay.Maghrib)),
        ],
        [
            CreateJumuahBlock(),
            CreatePrayerBlock(AppResources.Prayer_Asr, ETimeSection.Asr, nameof(MosquePrayerTimesDay.Asr)),
            CreatePrayerBlock(AppResources.Prayer_Isha, ETimeSection.Isha, nameof(MosquePrayerTimesDay.Isha)),
        ],
    ];

    /// <summary>
    /// The start of the prayer time and, as its sub time, the start of the congregational prayer,
    /// which is only shown when the mosque publishes one.
    /// </summary>
    private BlockDefinition CreatePrayerBlock(string name, ETimeSection section, string prayerTimeProperty, bool showEnd = false)
    {
        string binding = PrayerTimeBinding(prayerTimeProperty);

        return new BlockDefinition(
            name,
            section,
            binding,
            value =>
            {
                var prayerTime = value as MosquePrayerTime;
                string startTime = FormatTime(prayerTime?.Start) ?? MissingTime;

                return showEnd
                    ? $"{startTime} - {FormatTime(prayerTime?.End) ?? MissingTime}"
                    : startTime;
            },
            [
                new SubTimeDefinition(
                    AppResources.SubTime_Iqama,
                    binding,
                    value => FormatTime(GetCongregationStart(value as MosquePrayerTime)) ?? "",
                    ShowBinding: binding,
                    ShowConverter: value => GetCongregationStart(value as MosquePrayerTime) is not null)
            ]);
    }

    private BlockDefinition CreateJumuahBlock()
    {
        string jumuah2StartBinding = $"{PrayerTimeBinding(nameof(MosquePrayerTimesDay.Jumuah2))}.{nameof(GenericPrayerTime.Start)}";

        return new BlockDefinition(
            AppResources.Prayer_Jumuah,
            ETimeSection.General,
            PrayerTimeBinding(nameof(MosquePrayerTimesDay.Jumuah)),
            value => FormatTime((value as MosquePrayerTime)?.Start) ?? MissingTime,
            [],
            new SecondaryTimeDefinition(jumuah2StartBinding, value => FormatTime(value as ZonedDateTime?) ?? ""));
    }

    private static ZonedDateTime? GetCongregationStart(MosquePrayerTime? prayerTime)
    {
        if (prayerTime?.Start is not ZonedDateTime start || prayerTime.CongregationStartOffset <= 0)
        {
            return null;
        }

        return start.PlusMinutes(prayerTime.CongregationStartOffset);
    }

    private string? FormatTime(ZonedDateTime? value)
        => SystemInfoService.GetInCurrentZone(value)?.ToString(TimeFormat, null);

    private static string PrayerTimeBinding(string prayerTimeProperty)
        => $"{nameof(MosquePrayerTimeViewModel.PrayerTimesSet)}.{prayerTimeProperty}";
}
