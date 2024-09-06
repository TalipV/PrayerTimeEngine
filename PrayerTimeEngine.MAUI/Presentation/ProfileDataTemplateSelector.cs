using PrayerTimeEngine.Presentation.Views.MosquePrayerTime;
using PrayerTimeEngine.Presentation.Views.PrayerTimes;

namespace PrayerTimeEngine.Presentation
{
    public class ProfileDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrayerTimesTemplate { get; set; }
        public DataTemplate MosquePrayerTimeTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is DynamicPrayerTimeViewModel)
                return PrayerTimesTemplate;
            else if (item is MosquePrayerTimeViewModel)
                return MosquePrayerTimeTemplate;

            throw new NotImplementedException($"Type '{item.GetType().Name ?? "NULL"}' not supported");
        }
    }
}
