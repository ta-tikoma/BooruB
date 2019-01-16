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
                    Images.ClearSelf();
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

        // увеличение
        private void ZoomOpen(object sender, RoutedEventArgs e)
        {
            DetailImageZoom.Source = DetailImage.Source;
            DetailImageZoomGrid.Visibility = Visibility.Visible;
        }

        private void ZoomClose(object sender, RoutedEventArgs e)
        {
            DetailImageZoomGrid.Visibility = Visibility.Collapsed;
            DetailImageZoom.Source = null;
            DetailImageZoomScrollViewer.ChangeView(null, null, 1);
        }

        private void DetailImageZoomScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Point p = e.GetPosition(DetailImageZoomScrollViewer);
            TimeSpan period = TimeSpan.FromMilliseconds(10);

            Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (DetailImageZoomScrollViewer.ZoomFactor <= 1)
                    {
                        var k = DetailImageZoomScrollViewer.ChangeView(p.X + DetailImageZoomScrollViewer.HorizontalOffset * 2, p.Y + DetailImageZoomScrollViewer.VerticalOffset * 2, 2);
                    }
                    else
                    {
                        DetailImageZoomScrollViewer.ChangeView(DetailImageZoomScrollViewer.HorizontalOffset / 2 - p.X, DetailImageZoomScrollViewer.VerticalOffset / 2 - p.Y, 1);
                    }
                });
            }
            , period);
        }
        // увеличение

        // пагинация
        private void Continue(object sender, RoutedEventArgs e)
        {
            Images.ClearSelf(Models.Page.GetCurrentPage());
        }

        int page = 1;
        string next_page_link = "";
        private void Image_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Models.Image image = sender.DataContext as Models.Image;
            if (image == null)
            {
                page = 1;
                next_page_link = "";
                return;
            }

            if (image.Page > page)
            {
                Models.Page.Save(page, next_page_link);
                page = image.Page;
                next_page_link = image.NextPageLink;
            }

            //System.Diagnostics.Debug.WriteLine("Image_DataContextChanged:" + (sender.DataContext as Models.Image)?.Page);
        }

        // пагинация
    }
}
