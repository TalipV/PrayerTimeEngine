using PrayerTimeEngine.Presentation.GraphicsView;
using PrayerTimeEngine.Presentation.ViewModel;
using System.Windows.Input;

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

            this.searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;

            this.Popup.Opening += Popup_Opening;
            this.Popup.Opened += Popup_Opened;
            this.Popup.Closed += Popup_Closed;
            this.Popup.Closing += Popup_Closing;
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            this.Popup.IsOpen = true;
        }

        private void Popup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ;
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            ;
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            ;
        }

        private void Popup_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ;
        }

        private void ViewModel_OnAfterLoadingPrayerTimes_EventTrigger()
        {
            PrayerTimeGraphicView.DisplayPrayerTime = _viewModel.DisplayPrayerTime;
            PrayerTimeGraphicViewBase.Invalidate();
        }

        /// <summary>
        /// Triggers when the app is opened after being minimized
        /// </summary>
        private void app_Resumed()
        {
            _viewModel.OnActualAppearing();
        }

        /// <summary>
        /// Triggers when this page is navigated to from another page
        /// </summary>
        protected override void OnAppearing()
        {
            if (Application.Current is App app)
            {
                app.Resumed += app_Resumed;
            }

            _viewModel.OnActualAppearing();
        }

        protected override void OnDisappearing()
        {
            if (Application.Current is App app)
            {
                app.Resumed -= app_Resumed;
            }
        }
    }
}
