using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace BooruB.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Models.Images Images = new Models.Images();
        Models.TagGroups HistoryOfTags = Models.TagGroups.Load();

        public MainPage()
        {
            this.InitializeComponent();
            HideStatusBarAsync();

            ListLoadingRotation = RotationLoadingIcon;
            ListLoadingShow = ShowLoadingButton;
            ListLoadingHide = HideLoadingButton;

            NothingFoundShow = ShowNothingFoundText;
            NothingFoundHide = HideNothingFoundText;
            NothingFoundText = NothingFound;

            if ((Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
            {
                DetailScrollViewer.VerticalAlignment = VerticalAlignment.Top;
                if (App.Settings.hide_nav_button == null)
                {
                    App.Settings.hide_nav_button = false;
                }
            }
            else
            {
                if (App.Settings.hide_nav_button == null)
                {
                    App.Settings.hide_nav_button = true;
                }
            }

            if (App.Settings.hide_nav_button == true)
            {
                DetailCommandBar.PrimaryCommands.Remove(LeftButton);
                DetailCommandBar.PrimaryCommands.Remove(RightButton);
                DetailCommandBar.PrimaryCommands.Remove(CloseButton);
            }

            CurrentTab = TagsContainer;
            //AsyncInfo.Run((c) => Images.LoadMoreItemsAsync(c, 0));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
        }

        private async void HideStatusBarAsync()
        {
            try
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusBar.HideAsync();

                var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                view.TryEnterFullScreenMode();
            }
            catch (Exception ex)
            {
            }
        }

        private void Setting(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.SettingPage));
        }

        public static List<AppBarButton> AppBarButtons = new List<AppBarButton>();

        private void CommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppBarButtons.Clear();

            foreach (Models.Site site in Models.Site.Load())
            {
                AppBarButton appBarButton = new AppBarButton()
                {
                    Label = site.Name,
                    Tag = site.Url,
                    Icon = (site.Url == App.Settings.GetSite()?.Url) ? new SymbolIcon(Symbol.Accept) : null
                };

                appBarButton.Click += (_s, _e) =>
                {
                    App.Settings.current_site = site.Url;
                    App.Settings.current_tag_code = null;
                    //Images.ClearSelf();
                    ImagesGrid.Reset();
                    HistoryOfTags.ChaneCurrentTag();
                };
                MainCommandBar.SecondaryCommands.Add(appBarButton);
                AppBarButtons.Add(appBarButton);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (App.Settings.max_side_size == -1)
            {
                if ((Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
                {
                    App.Settings.max_side_size = 230;
                }
                else
                {
                    App.Settings.max_side_size = 100;
                }
            }

            double width = Window.Current.Bounds.Width;
            App.Settings.images_in_row = (int) (width / App.Settings.max_side_size);
            App.Settings.side_size = (int) (width / App.Settings.images_in_row) - 4;
        }

        // быстрое сообщение
        private void ShowMessage(string message) {
            Message.Text = message;
            AcceptLinkIconTransformFromUp.Begin();
        }
        // быстрое сообщение

        // пагинация
        private void Continue(object sender, RoutedEventArgs e)
        {
            ImagesGrid.Reset(Models.Page.GetCurrentPage());
        }
        // пагинация
    }
}
