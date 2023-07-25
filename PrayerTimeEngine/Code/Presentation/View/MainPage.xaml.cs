using PrayerTimeEngine.Code.Presentation.ViewModel;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            (BindingContext as MainPageViewModel).OnAppearing();
        }
    }
}
