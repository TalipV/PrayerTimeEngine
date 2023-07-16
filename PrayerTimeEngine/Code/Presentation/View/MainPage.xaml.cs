using Microsoft.Maui.Controls;
using PrayerTimeEngine.Presentation.ViewModels;

namespace PrayerTimeEngine
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
