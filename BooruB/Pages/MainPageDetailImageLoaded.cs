using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
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

        private async Task<StorageFile> GetFile(string url)
        {
            StorageFile tempFile = null;

            string type = GetType(url);
            //string name = GetName(url, type);
            string hash = Models.Page.GetHash2(url);
            //System.Diagnostics.Debug.WriteLine(hash + type);

            try
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(hash + type);
                DetailImageProgressTextBlock.Opacity = 0.0;
                //System.Diagnostics.Debug.WriteLine("from temp");
            }
            catch (Exception)
            {
                SetProgressValue(0);
                DetailImageProgressTextBlock.Opacity = 1.0;
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(hash + type, CreationCollisionOption.OpenIfExists);
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

        private void ImageShow_Completed(object sender, object e)
        {
            SwipeTextPanel.Height = DetailScrollViewer.ActualHeight;
            SaveButton.IsEnabled = true;
            ShareButton.IsEnabled = true;
        }

        private void DownloadProgress(DownloadOperation obj)
        {
            SetProgressValue((int)(obj.Progress.BytesReceived / (obj.Progress.TotalBytesToReceive / 100)));
        }

        private void DetailImageProgressLoaded(object sender, RoutedEventArgs e)
        {
            DetailImageProgress = sender as TextBlock;
        }
    }
}
