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
            Helpers.LoadingAnimation.RotationLoadingIcon = RotationLoadingIcon; 
            Helpers.LoadingAnimation.ShowLoadingButton = ShowLoadingButton; 
            Helpers.LoadingAnimation.HideLoadingButton = HideLoadingButton;
            Helpers.LoadingAnimation.ShowNothingFound = ShowNothingFound;
            Helpers.LoadingAnimation.HideNothingFound = HideNothingFound;
        }

        private async void HideStatusBarAsync()
        {
            try
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusBar.HideAsync();

                //var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                //view.TryEnterFullScreenMode();
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
            //App.Settings.zoom_side_size = (int) width - 26;
        }

        private void SetLeftRightButtonsEnable()
        {
            if (ImageData == null)
            {
                return;
            }

            if (ImageData.Number == 1)
            {
                LeftButton.IsEnabled = false;
            }
            else
            {
                LeftButton.IsEnabled = true;
            }

            if (ImageData.Number == Images.Count)
            {
                RightButton.IsEnabled = false;
            } else
            {
                RightButton.IsEnabled = true;
            }
        }

        // детальная открытие скрытие панели
        Models.Image ImageData = null;
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageData = await ((sender as Image).DataContext as Models.Image).DetailDataLoad();
            if (ImageData != null)
            {
                SetLeftRightButtonsEnable();
                Detail.Visibility = Visibility.Visible;
                DetailImageProgressTextBlock.Opacity = 1;
                SetProgressValue(0);
                ShowDetail.Begin();
            } else
            {
                ShowMessage("Something wrong!");
            }
        }

        private void DetailClose(object sender, RoutedEventArgs e)
        {
            HideDetail.Begin();
            Detail.DataContext = null;
        }

        private void DetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DetailClose(sender, new RoutedEventArgs());
        }

        private void HideDetail_Completed(object sender, object e)
        {
            Detail.Visibility = Visibility.Collapsed;
            //DetailScrollViewerZoom.ChangeView(null, null, 1);
        }

        private void ShowDetail_Completed(object sender, object e)
        {
            Detail.DataContext = ImageData;
        }
        // детальная открытие скрытие панели

        // сохранение
        private string GetType(string url)
        {
            foreach (string type in new[] { ".jpeg", ".jpg", ".gif", ".bmp" })
            {
                if (url.IndexOf(type) != -1)
                {
                    return type;
                }
            }
            return ".png";
        }

        private string GetName(string url, string type)
        {
            string name = "image";
            string clearType = type.Substring(1);
            string[] parts = url.Split(new char[] { '.', '/' });
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i] == clearType)
                {
                    return parts[i - 1];
                }
            }
            return name;
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = (sender as AppBarButton).DataContext as string;
                string type = GetType(url);
                string name = GetName(url, type);
                // определяем тип

                // определяем тип

                StorageFile file = null;
                
                // путь из настроек
                if (App.Settings.image_save_path.Length > 0)
                {
                    try
                    {
                        StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(App.Settings.image_save_path + '\\');
                        file = await storageFolder.CreateFileAsync(name + type, CreationCollisionOption.GenerateUniqueName);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                // путь из настроек

                if (file == null)
                {
                    // диалог сохранения
                    var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                    savePicker.FileTypeChoices.Add("Images", new List<string>() { type });
                    savePicker.SuggestedFileName = name;
                    file = await savePicker.PickSaveFileAsync();
                    // диалог сохранения
                }

                if (file != null)
                {
                    StorageFile tempFile = await GetFile(url);
                    await tempFile.CopyAndReplaceAsync(file);

                    /*
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation download = downloader.CreateDownload(new Uri(url), file);

                    SaveIconToDown.Completed += OnHideSaveIcon;
                    SaveIconToDown.Begin();

                    await download.StartAsync().AsTask();
                    */
                    ShowMessage("Saved!");
                    /*
                    SaveIconToDown.Completed += OnHideSyncIcon;
                    SaveIconToDown.Begin();
                    */
                } else
                {
                    ShowMessage("File not choose!");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void OnHideSaveIcon(object sender, object e)
        {
            SaveIconToDown.Completed -= OnHideSaveIcon;
            DetailSaveIcon.Symbol = Symbol.Sync;
            SaveIconRotation.Begin();
            SaveIconFromUp.Begin();
        }

        private void OnHideSyncIcon(object sender, object e)
        {
            SaveIconToDown.Completed -= OnHideSyncIcon;
            SaveIconRotation.Stop();
            DetailSaveIcon.Symbol = Symbol.Save;
            SaveIconFromUp.Begin();
        }
        // сохранение

        // копирование ссылки
        private void CopyPageLink(object sender, object e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(App.Settings.current_site + ((sender as AppBarButton).DataContext as string));
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            ShowMessage("Сopied!");
        }

        private void CopyImageLink(object sender, object e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText((sender as AppBarButton).DataContext as string);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            ShowMessage("Сopied!");
        }
        // копирование ссылки

        // быстрое сообщение
        private void ShowMessage(string message) {
            Message.Text = message;
            AcceptLinkIconTransformFromUp.Begin();
        }
        // быстрое сообщение

        // пошарить
        private void Share(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = ImageData.DetailImageUrl;
                string type = GetType(url);

                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += async (_s, _args) =>
                {
                    var deferral = _args.Request.GetDeferral();

                    var tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("image" + type, CreationCollisionOption.ReplaceExisting);
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation download = downloader.CreateDownload(new Uri(url), tempFile);
                    await download.StartAsync().AsTask();

                    _args.Request.Data.Properties.Title = "Image";
                    _args.Request.Data.Properties.Description = App.Settings.current_site;
                    _args.Request.Data.SetStorageItems(new IStorageItem[] { tempFile });

                    deferral.Complete();
                };
                DataTransferManager.ShowShareUI();
            }
            catch (Exception)
            {
                ShowMessage("Something wrone!");
            }
        }
        // пошарить

        // перезагрузить
        private void Reload(object sender, RoutedEventArgs e)
        {
            HideDetail.Completed += HideDetailReload;
            HideDetail.Begin();
            Detail.DataContext = null;
        }

        private async void HideDetailReload(object sender, object e)
        {
            HideDetail.Completed -= HideDetailReload;
            ImageData = await ImageData.DetailDataReLoad();
            if (ImageData != null)
            {
                Detail.Visibility = Visibility.Visible;
                ShowDetail.Begin();
            }
            else
            {
                ShowMessage("Something wrong!");
            }
        }
        // перезагрузить

        // поиск анимация показа\скрытия
        private void ShowSearchPanel(object sender, RoutedEventArgs e)
        {
            if (Search.Visibility == Visibility.Collapsed)
            {
                Search.Visibility = Visibility.Visible;
                ShowSearch.Begin();
            } else
            {
                HideSearch.Begin();
            }
        }

        private void HideSearchIconComplited(object sender, object e)
        {
            FindIcon.Symbol = Symbol.Cancel;
            ShowSearch2.Begin();
        }

        private void HideCanselIconComplited(object sender, object e)
        {
            FindIcon.Symbol = Symbol.Find;
            HideSearch2.Begin();
        }

        private void SearchBorderHideCompleted(object sender, object e)
        {
            Search.Visibility = Visibility.Collapsed;
        }
        // поиск анимация показа\скрытия

        // поле тега
        private void TagClick(object sender, RoutedEventArgs e)
        {
            menuTag = (sender as Button).DataContext as Models.Tag;
            OpenTag_Click(sender, e);
        }

        private void SearchInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchInputOpenTag(sender, new RoutedEventArgs());
            }
        }

        public void SearchInputAddTag(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = SearchInput.Text,
                Code = SearchInput.Text.ToLower(),
                SiteUrl = App.Settings.current_site
            };
            SearchInput.Text = "";
            AddToTags_Click(sender, e);
        }

        private void SearchInputOpenTag(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = SearchInput.Text,
                Code = SearchInput.Text.ToLower(),
                SiteUrl = App.Settings.current_site
            };
            SearchInput.Text = "";
            OpenTag_Click(sender, e);
        }
        // поле тега

        // теги меню
        Models.Tag menuTag = null;
        private void TagHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (tagMenuIsOpen)
            {
                return;
            }
            Button button = sender as Button;
            menuTag = button.DataContext as Models.Tag;

            if (HistoryOfTags.HasTag(menuTag))
            {
                MenuFlyoutSeparator.Visibility = Visibility.Visible;
                RemoveFromHistory.Visibility = Visibility.Visible;
            } else
            {
                MenuFlyoutSeparator.Visibility = Visibility.Collapsed;
                RemoveFromHistory.Visibility = Visibility.Collapsed;
            }

            if (App.Settings.current_site != menuTag.SiteUrl)
            {
                OpenTagOnThisSIte.Visibility = Visibility.Visible;
            }
            else
            {
                OpenTagOnThisSIte.Visibility = Visibility.Collapsed;
            }

            string[] tags = new string[] { };
            if (App.Settings.current_tag_code != null)
            {
                tags = App.Settings.current_tag_code.Split(new char[] { '+' }).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }

            if (tags.Contains(menuTag.Code))
            {
                OpenTag.Visibility = Visibility.Collapsed;
                AddToTags.Visibility = Visibility.Collapsed;
                RemoveFromTags.Visibility = Visibility.Visible;
            } else
            {
                foreach (string code in tags)
                {
                    System.Diagnostics.Debug.WriteLine("code_" + code);
                }
                if (tags.Count() == 0)
                {
                    AddToTags.Visibility = Visibility.Collapsed;
                } else
                {
                    AddToTags.Visibility = Visibility.Visible;
                }
                RemoveFromTags.Visibility = Visibility.Collapsed;
            }

            TagMenu.ShowAt(button, e.GetPosition(button));
        }

        bool tagMenuIsOpen = false;
        private void TagMenu_Opened(object sender, object e)
        {
            tagMenuIsOpen = true;
        }

        private void TagMenu_Closed(object sender, object e)
        {
            tagMenuIsOpen = false;
        }
        // теги меню
        
        // теги действия
        private void HidePanels()
        {
            if (Detail.DataContext != null)
            {
                HideDetail.Begin();
                Detail.DataContext = null;
            }

            if (Search.Visibility == Visibility.Visible)
            {
                HideSearch.Begin();
            }
        }

        private void OpenTagOnThisSIte_Click(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = menuTag.Name,
                Code = menuTag.Code,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void OpenTag_Click(object sender, RoutedEventArgs e)
        {
            HidePanels();
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code + ":");
            System.Diagnostics.Debug.WriteLine("menuTag.SiteUrl:" + menuTag.SiteUrl + ":");
            if (menuTag.Code == "")
            {
                App.Settings.current_tag_code = null;
            } else
            {
                HistoryOfTags.AddTag(menuTag);
                App.Settings.current_site = menuTag.SiteUrl;
                App.Settings.current_tag_code = menuTag.Code;
            }
            Images.ClearSelf();
            menuTag = null;
            HistoryOfTags.ChaneCurrentTag();
        }

        private void AddToTags_Click(object sender, RoutedEventArgs e)
        {
            List<string> tags = new List<string>();
            if (App.Settings.current_tag_code != null)
            {
                tags = App.Settings.current_tag_code.Split(new char[] { '+' }).ToList();
            }

            System.Diagnostics.Debug.WriteLine("App.Settings.current_tag_code:" + App.Settings.current_tag_code);

            tags.Add(menuTag.Code);
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code);

            string newTagCode = String.Join("+", tags.OrderBy(x => x).Distinct().ToArray());
            System.Diagnostics.Debug.WriteLine("newTagCode:" + newTagCode);

            menuTag = new Models.Tag()
            {
                Name = newTagCode,
                Code = newTagCode,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void RemoveFromTags_Click(object sender, RoutedEventArgs e)
        {
            List<string> tags = App.Settings.current_tag_code.Split(new char[] { '+' }).ToList();
            tags.Remove(menuTag.Code);
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code);

            string newTagCode = String.Join("+", tags.OrderBy(x => x).Distinct().ToArray());
            System.Diagnostics.Debug.WriteLine("newTagCode:" + newTagCode);

            menuTag = new Models.Tag()
            {
                Name = newTagCode,
                Code = newTagCode,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void RemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryOfTags.RemoveTag(menuTag);
            menuTag = null;
        }
        // теги действия

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

        private void Right(object sender, RoutedEventArgs e)
        {
            Detail.DataContext = null;
            LeftToRightBeginAnimation.To = -Detail.ActualWidth;
            LeftToRightBegin.Begin();
        }

        private void Left(object sender, RoutedEventArgs e)
        {
            Detail.DataContext = null;
            LeftToRightBeginAnimation.To = Detail.ActualWidth;
            LeftToRightBegin.Begin();
        }

        private async void LeftToRightBeginCompleted(object sender, object e)
        {
            Models.Image current = ImageData;
            ImageData = null;
            if (LeftToRightBeginAnimation.To == Detail.ActualWidth)
            {
                for (int i = 1; i < Images.Count; i++)
                {
                    if (Images[i] == current)
                    {
                        ImageData = await Images[i - 1].DetailDataLoad(); 
                        break;
                    }
                }
                LeftToRightEndAnimation.From = -Detail.ActualWidth;
            } else
            {
                for (int i = 0; i < Images.Count - 1; i++)
                {
                    if (Images[i] == current)
                    {
                        ImageData = await Images[i + 1].DetailDataLoad(); 
                        break;
                    }
                }
                LeftToRightEndAnimation.From = Detail.ActualWidth;
            }

            if (ImageData == null)
            {
                ShowMessage("Something wrong!");
                LeftToRightEnd.Completed += LeftToRightEndCompleted;
            } else
            {
                SetLeftRightButtonsEnable();
                Detail.DataContext = ImageData;
            }
            LeftToRightEnd.Begin();
        }

        private void LeftToRightEndCompleted(object sender, object e)
        {
            LeftToRightEnd.Completed -= LeftToRightEndCompleted;
            HideDetail.Begin();
            Detail.DataContext = null;
        }

        // загрузка изображения
        public CancellationTokenSource cts = null;
        private TextBlock DetailImageProgress = null;
        BitmapImage bitmapImage = new BitmapImage();

        public void SetProgressValue(int value)
        {
            if (DetailImageProgress == null)
            {
                return;
            }
            DetailImageProgress.Text = value + " %";
        }

        /*private async void DetailImage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext == null)
            {
                DetailImage.Source = null;
                return;
            }

            try
            {
                DetailImageProgressTextBlock.Opacity = 1.0;

                //System.Diagnostics.Debug.WriteLine("DetailImage_DataContextChanged");
                //System.Diagnostics.Debug.WriteLine("DataContext:" + (sender.DataContext));
                Models.Image image = sender.DataContext as Models.Image;

                using (cts = new CancellationTokenSource())
                {
                    IBuffer buffer = await App.Settings.Query.GetFileStatic(image.DetailImageUrl, this);

                    if (buffer != null)
                    {
                        using (var stream = new InMemoryRandomAccessStream())
                        {
                            stream.WriteAsync(buffer).GetResults();
                            stream.Seek(0);

                            await bitmapImage.SetSourceAsync(stream);
                            DetailImage.Source = bitmapImage;
                            ShowImageHeight.To = DetailStackPanel.ActualWidth / bitmapImage.PixelWidth * bitmapImage.PixelHeight;
                            ShowImage.Begin();
                            //System.Diagnostics.Debug.WriteLine("DetailImage.Source");
                        }
                        buffer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                DetailImageProgressTextBlock.Opacity = 0.0;
                ShowMessage("Uncorrect image");
                System.Diagnostics.Debug.WriteLine("ex:" + ex.Message);
            }
        }*/

        private async Task<StorageFile> GetFile(string url)
        {
            StorageFile tempFile = null;

            string type = GetType(url);
            string name = GetName(url, type);

            try
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(name + type);
                DetailImageProgressTextBlock.Opacity = 0.0;
                //System.Diagnostics.Debug.WriteLine("from temp");
            }
            catch (Exception)
            {
                DetailImageProgressTextBlock.Opacity = 1.0;
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(name + type, CreationCollisionOption.OpenIfExists);
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(new Uri(url), tempFile);
                using (cts = new CancellationTokenSource())
                {
                    await download.StartAsync().AsTask(cts.Token, new Progress<DownloadOperation>(DownloadProgress));
                }
                downloader = null;
                download = null;
                //System.Diagnostics.Debug.WriteLine("from net");
            }

            return tempFile;
        }

        private async void DetailImage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext == null)
            {
                DetailImage.Source = null;
                return;
            }

            try
            {
                Models.Image image = sender.DataContext as Models.Image;

                StorageFile tempFile = await GetFile(image.DetailImageUrl);

                using (IRandomAccessStream fileStream = await tempFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    await bitmapImage.SetSourceAsync(fileStream);
                }

                DetailImage.Source = bitmapImage;
                ShowImageHeight.To = DetailStackPanel.ActualWidth / bitmapImage.PixelWidth * bitmapImage.PixelHeight;
                ShowImage.Begin();
            }
            catch (Exception ex)
            {
                DetailImageProgressTextBlock.Opacity = 0.0;
                ShowMessage("Uncorrect image");
                //System.Diagnostics.Debug.WriteLine("ex:" + ex.Message);
            }
        }

        private void DownloadProgress(DownloadOperation obj)
        {
            SetProgressValue((int) (obj.Progress.BytesReceived / (obj.Progress.TotalBytesToReceive / 100)));
        }

        private void DetailImageProgressLoaded(object sender, RoutedEventArgs e)
        {
            DetailImageProgress = sender as TextBlock;
        }
        // загрузка изображения

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

        // свайп скрола
        // свайп скрола
    }
}
