using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        // детальная: открытие скрытие панели
        Models.Image ImageData = null;
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageData = await ((sender as Image).DataContext as Models.Image).DetailDataLoad();
            if (ImageData != null)
            {
                PrepareToOpen();

                SetLeftRightButtonsEnable();
                Detail.Visibility = Visibility.Visible;
                DetailImageProgressTextBlock.Opacity = 1;
                SetProgressValue(0);
                ShowDetail.Begin();
            }
            else
            {
                ShowMessage("Something wrong!");
            }
        }

        private void DetailClose(object sender, RoutedEventArgs e)
        {
            HideDetail.Begin();
            DetailStackPanelTranslateY.TranslateY = 0;
            SwipeTextPanel.Visibility = Visibility.Collapsed;
            Detail.DataContext = null;
        }

        private void DetailScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
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
            SwipeTextPanel.Height = DetailScrollViewer.ActualHeight;
            SwipeTextPanel.Visibility = Visibility.Visible;
        }

        private void PrepareToOpen()
        {
            SaveButton.IsEnabled = false;
            ShareButton.IsEnabled = false;
        }
    }
}
