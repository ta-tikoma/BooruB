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
        // детальная : предыдущий следующий слайд
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
            }
            else
            {
                RightButton.IsEnabled = true;
            }
        }

        private void Right(object sender, RoutedEventArgs e)
        {
            PrepareToOpen();
            SwipeTextPanel.Visibility = Visibility.Collapsed;
            DetailStackPanelTranslateY.TranslateY = 0;

            Detail.DataContext = null;
            LeftToRightBeginAnimation.To = -Detail.ActualWidth;
            LeftToRightBegin.Begin();
        }

        private void Left(object sender, RoutedEventArgs e)
        {
            PrepareToOpen();
            SwipeTextPanel.Visibility = Visibility.Collapsed;
            DetailStackPanelTranslateY.TranslateY = 0;

            Detail.DataContext = null;
            LeftToRightBeginAnimation.To = Detail.ActualWidth;
            LeftToRightBegin.Begin();
        }

        bool somethingWrong = false;
        private async void LeftToRightBeginCompleted(object sender, object e)
        {
            Models.Image current = ImageData;
            ImageData = null;
            string message = "Something wrong!";
            if (LeftToRightBeginAnimation.To == Detail.ActualWidth)
            {
                message = "It first image";
                for (int i = 1; i < Images.Count; i++)
                {
                    if (Images[i] == current)
                    {
                        ImageData = await Images[i - 1].DetailDataLoad();
                        break;
                    }
                }
                LeftToRightEndAnimation.From = -Detail.ActualWidth;
            }
            else
            {
                message = "It last image";
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
                somethingWrong = true;
                ShowMessage(message);
            }
            else
            {
                somethingWrong = false;
                SetLeftRightButtonsEnable();
                Detail.DataContext = ImageData;
            }
            LeftToRightEnd.Begin();
        }

        private void LeftToRightEndCompleted(object sender, object e)
        {
            if (somethingWrong)
            {
                HideDetail.Begin();
                Detail.DataContext = null;
                somethingWrong = false;
            } else
            {
                SwipeTextPanel.Height = DetailScrollViewer.ActualHeight;
                SwipeTextPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
