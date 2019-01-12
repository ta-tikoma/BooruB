using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace BooruB.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        ObservableCollection<VModels.SiteSettings> Sites = VModels.SiteSettings.Load();

        public SettingPage()
        {
            this.InitializeComponent();
            Sites.CollectionChanged += (_s, _e) =>
            {
                VModels.SiteSettings.Save(Sites);
            };
            ShowStatusBarAsync();
        }

        private async void ShowStatusBarAsync()
        {
            try
            {
                //var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                //await statusBar.ShowAsync();

                //var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                //view.ExitFullScreenMode();
            }
            catch (Exception ex)
            {
            }
        }

        // быстрое сообщение
        private void ShowMessage(string message)
        {
            Message.Text = message;
            AcceptLinkIconTransformFromUp.Begin();
        }
        // быстрое сообщение

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.SiteSettings site = button.DataContext as VModels.SiteSettings;
            site.site.RemoveTags();
            site.site.RemovePage();
            Sites.Remove(site);
            App.Settings.current_site = null;
            ShowMessage("Site removed!");
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            Message.Text = "";

            Uri uri = null;
            try
            {
                uri = new Uri(InputUrl.Text);
            }
            catch (Exception)
            {
            }

            if (uri == null)
            {
                ShowMessage("Uncorrect link!");
            } else
            {
                InputUrl.Text = "";
                Sites.Add(new VModels.SiteSettings(new Models.Site()
                {
                    Url = uri.Scheme + "://" + uri.Host + "/",
                    Name = uri.Host,
                }));
                ShowMessage("Site added!");
            }
        }

        private void InputUrl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Add(sender, e);
            }
        }

        private async void SelectSavePath(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ImageSavePath.Text = folder.Path;
            }
            else
            {
                ImageSavePath.Text = "";
            }
        }

        private void MaxSideSizeChaned(object sender, RangeBaseValueChangedEventArgs e)
        {
            double width = Window.Current.Bounds.Width;
            App.Settings.images_in_row = (int) (width / App.Settings.max_side_size);
        }

        private void ProxySelectorToggle(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VModels.SiteSettings site = button.DataContext as VModels.SiteSettings;
            if (site.ProxySelectorVisibility == Visibility.Visible)
            {
                site.ProxySelectorVisibility = Visibility.Collapsed;
            } else
            {
               site.ProxySelectorVisibility = Visibility.Visible;
            }
        }

        private void ClearAllSettings(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            this.Frame.Navigate(typeof(Pages.SettingPage));
        }

        private async void InputUrl_Loaded(object sender, RoutedEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                (sender as TextBox).Text = await dataPackageView.GetTextAsync();
            }
        }
    }
}
