﻿using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Main;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes
{
    [AddINotifyPropertyChangedInterface]
    public class DynamicPrayerTimeViewModel(
            IDynamicPrayerTimeProviderManager dynamicPrayerTimeProviderManager,
            IDispatcher dispatcher,
            IProfileService profileService
        ) : IPrayerTimeViewModel
    {
        public MainPageViewModel MainPageViewModel { get; set; }
        public Profile Profile { get; set; }
        public PrayerTimesCollection PrayerTimesCollection { get; set; }

        public bool ShowFajrGhalas { get; set; }
        public bool ShowFajrRedness { get; set; }
        public bool ShowMithlayn { get; set; }
        public bool ShowKaraha { get; set; }
        public bool ShowIshtibaq { get; set; }
        public bool ShowMaghribSufficientTime { get; set; }

        public AbstractPrayerTime GetDisplayPrayerTime(Instant instant)
        {
            var prayerTimeBundle = this.PrayerTimesCollection;

            // only show data when no information is lacking
            if (prayerTimeBundle is null || prayerTimeBundle.AllPrayerTimes.Any(x => x.Start is null || x.End is null))
            {
                return null;
            }

            return prayerTimeBundle.AllPrayerTimes.FirstOrDefault(x => x.Start.Value.ToInstant() <= instant && instant <= x.End.Value.ToInstant())
                ?? prayerTimeBundle.AllPrayerTimes.OrderBy(x => x.Start.Value.ToInstant()).FirstOrDefault(x => x.Start.Value.ToInstant() > instant);
        }

        public async Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
        {
            await showHideSpecificTimes(Profile);

            PrayerTimesCollection =
                await dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(
                    Profile.ID,
                    zonedDateTime,
                    cancellationToken);
        }

        private async Task showHideSpecificTimes(Profile profile)
        {
            if (profile is null)
                return;

            await dispatcher.DispatchAsync(() =>
            {
                ShowFajrGhalas = isCalculationShown(profile, ETimeType.FajrGhalas);
                ShowFajrRedness = isCalculationShown(profile, ETimeType.FajrKaraha);

                ShowMithlayn = isCalculationShown(profile, ETimeType.AsrMithlayn);
                ShowKaraha = isCalculationShown(profile, ETimeType.AsrKaraha);

                ShowMaghribSufficientTime = isCalculationShown(profile, ETimeType.MaghribSufficientTime);
                ShowIshtibaq = isCalculationShown(profile, ETimeType.MaghribIshtibaq);
            });
        }

        private bool isCalculationShown(Profile profile, ETimeType timeData)
        {
            return profileService.GetTimeConfig(profile, timeData)?.IsTimeShown == true;
        }
    }
}
