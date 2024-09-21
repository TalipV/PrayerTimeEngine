using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.MosquePrayerTimes;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;

namespace PrayerTimeEngine.Presentation.Views;

public class PrayerTimeViewModelFactory(
        IServiceProvider serviceProvider
    )
{
    public IPrayerTimeViewModel Create(Profile profile)
    {
        return profile switch
        {
            // get service from serviceProvider with all its dependencies + add profile to constructor
            MosqueProfile => ActivatorUtilities.CreateInstance<MosquePrayerTimeViewModel>(serviceProvider, profile),
            DynamicProfile => ActivatorUtilities.CreateInstance<DynamicPrayerTimeViewModel>(serviceProvider, profile),
            _ => throw new InvalidOperationException($"Unknown profile type '{profile.GetType().FullName}'")
        };
    }
}
