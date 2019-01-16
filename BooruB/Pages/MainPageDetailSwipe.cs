using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        // закрытие по свайпу
        private void DetailScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            /*
            if (DetailScrollViewer.VerticalOffset == 0)
            {
                CloseAnimation.Begin();
            }
            else
            {
                CloseAnimation.Stop();
            }*/
            //System.Diagnostics.Debug.WriteLine("HorizontalOffset:" + DetailScrollViewer.HorizontalOffset);
            //var ttv = DetailStackPanel.TransformToVisual(Window.Current.Content);
            //Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            //System.Diagnostics.Debug.WriteLine("DetailScrollViewer_ViewChanged:" + screenCoords.Y);
            SwipeAnimation.Stop();
        }

        double vOffset = 0;
        private void DetailScrollViewer_DirectManipulationStarted(object sender, object e)
        {
            /*
            SwipeTextPanel.Height = DetailStackPanel.ActualHeight;
            SwipeTextPanel.Visibility = Visibility.Visible;
            if (DetailScrollViewer.VerticalOffset != vOffset)
            {
                SwipeAnimation.Stop();
            } else
            {
                SwipeAnimation.Begin();
            }
            vOffset = DetailScrollViewer.VerticalOffset;
            */
        }

        private void DetailScrollViewer_DirectManipulationCompleted(object sender, object e)
        {
            //var ttv = DetailStackPanel.TransformToVisual(Window.Current.Content);
            //Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            //System.Diagnostics.Debug.WriteLine("DetailScrollViewer_DirectManipulationCompleted:" + screenCoords.Y);
            SwipeAnimation.Stop();
        }

        private void SwipeAnimation_Completed(object sender, object e)
        {
            /*var ttv = DetailStackPanel.TransformToVisual(SwipeTextPanel);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (screenCoords.Y > 30)
            {
                DetailClose(sender, new RoutedEventArgs());
            } else if (screenCoords.X > 30)
            {
                Left(sender, new RoutedEventArgs());
            } else if (screenCoords.X < -30)
            {
                Right(sender, new RoutedEventArgs());
            }*/

            //System.Diagnostics.Debug.WriteLine("CloseAnimation_Completed:Y:" + screenCoords.Y);
            //System.Diagnostics.Debug.WriteLine("CloseAnimation_Completed:X:" + screenCoords.X);
        }





        Point prevPoint = new Point();
        private void DetailScrollViewer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            prevPoint = e.Position;
            DetailScrollViewerTranslateYToZero.Stop();
        }

        private void DetailScrollViewer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double delta = (int) e.Position.Y - (int) prevPoint.Y;
            if (delta == 0)
            {
                return;
            }

            double y = DetailStackPanelTranslateY.TranslateY + delta;
            if ((y < 0) && (y > DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight) && (DetailScrollViewerTranslateY.TranslateY == 0))
            {
                DetailStackPanelTranslateY.TranslateY = y;
            } else
            {
                double delta2 = (e.Position.Y - prevPoint.Y) > 0 ? 1 : -1;
                double y2 = DetailScrollViewerTranslateY.TranslateY + delta2;
                if (Math.Abs(y2) <= 42)
                {
                    DetailScrollViewerTranslateY.TranslateY = y2;
                    if (y2 > 0)
                    {
                        CloseLineTransform.ScaleX = (float) (42f / 42f) * y2;
                        if (CloseLineTransform.ScaleX == 42)
                        {
                            DetailClose(sender, new RoutedEventArgs());
                        }
                    }
                }
            }
            prevPoint = e.Position;
        }

        private void DetailScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (DetailScrollViewerTranslateY.TranslateY != 0)
            {
                DetailScrollViewerTranslateYToZero.Begin();
            }
        }

        private void DetailScrollViewerTranslateYToZero_Completed(object sender, object e)
        {
            DetailScrollViewerTranslateY.TranslateY = 0;
        }
    }
}
