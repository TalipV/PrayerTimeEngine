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
        }

        protected override void OnAppearing()
        {
            (BindingContext as MainPageViewModel).OnAppearing();
            this.PrayerTimeGraphic.DisplayPrayerTime = _viewModel.DisplayPrayerTime;
        }
    }
}
