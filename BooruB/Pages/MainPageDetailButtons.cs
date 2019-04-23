using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        // детальная : кнопки
        // сохранение

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
                string type = Models.Image.GetType(url);
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
                    ShowMessage("Saved!");
                }
                else
                {
                    ShowMessage("File not choose!");
                }
            }
            catch (Exception ex)
            {
            }
        }

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

        // пошарить
        private void Share(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = ImageData.DetailImageUrl;

                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += async (_s, _args) =>
                {
                    var deferral = _args.Request.GetDeferral();

                    StorageFile tempFile = await GetFile(url);
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
    }
}
