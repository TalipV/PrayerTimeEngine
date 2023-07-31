using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this._viewModel = viewModel;
            viewModel.OnAfterLoadingPrayerTimes_EventTrigger += ViewModel_OnAfterLoadingPrayerTimes_EventTrigger;
        }

        private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
        {
            PrayerTimeGraphicView.DisplayPrayerTime = _viewModel.DisplayPrayerTime;
            PrayerTimeGraphicViewBase.Invalidate();
        }

        protected override void OnAppearing()
        {
            (BindingContext as MainPageViewModel).OnAppearing();
        }
    }
}
