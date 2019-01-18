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
        // колесико мыши
        private void DetailScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            double deltaY = e.GetCurrentPoint(sender as Grid).Properties.MouseWheelDelta / 2;
            //System.Diagnostics.Debug.WriteLine("DetailScrollViewer_PointerWheelChanged:" + deltaY);
            double y = DetailStackPanelTranslateY.TranslateY + deltaY;
            if ((y < 0) && (y > DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight))
            {
                DetailStackPanelTranslateY.TranslateY = y;
            } else
            {
                if (y < 0)
                {
                    DetailStackPanelTranslateY.TranslateY = DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight;
                }
                if (y > DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight)
                {
                    DetailStackPanelTranslateY.TranslateY = 0;
                }
            }
        }

        // начало
        Point prevPoint = new Point();
        bool? isVertical = null;
        bool isBeginAction = false;
        private void DetailScrollViewer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                return;
            }

            prevPoint = e.Position;
            DetailScrollViewerTranslateYToZero.Stop();
            dispatcherTimer.Stop();
            isBeginAction = false;
        }

        // дельта
        double slow = 0.1;
        double lastDelta = 0;

        const double hMax = 42;
        const double hDelta = 2;
        private void DetailScrollViewer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                return;
            }

            // определяем направление
            if (isVertical == null)
            {
                isVertical = Math.Abs(e.Position.Y - prevPoint.Y) > Math.Abs(e.Position.X - prevPoint.X);
            }

            // вертикальное
            if (isVertical == true)
            {
                double deltaY = ((int) e.Position.Y - (int) prevPoint.Y) * slow;
                lastDelta = deltaY;

                if (deltaY == 0)
                {
                    return;
                }

                double y = DetailStackPanelTranslateY.TranslateY + deltaY;
                if (slow < 1)
                {
                    slow += 0.1;
                }

                if ((y < 0) && (y > DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight) && (DetailScrollViewerTranslateY.TranslateY == 0))
                {
                    DetailStackPanelTranslateY.TranslateY = y;
                }
                else
                {
                    double delta2 = deltaY > 0 ? 1 : -1;
                    double y2 = DetailScrollViewerTranslateY.TranslateY + delta2;
                    if (Math.Abs(y2) <= 42)
                    {
                        DetailScrollViewerTranslateY.TranslateY = y2;
                        if (y2 > 0)
                        {
                            CloseLineTransform.ScaleX = (float)(42f / 42f) * y2;
                            if (CloseLineTransform.ScaleX == 42)
                            {
                                isBeginAction = true;
                                DetailClose(null, new RoutedEventArgs());
                            }
                        }
                    }
                }
            }

            // горизонтальное
            if (isVertical == false)
            {
                double deltaX = (int)e.Position.X - (int)prevPoint.X;
                if (deltaX == 0)
                {
                    return;
                }

                //System.Diagnostics.Debug.WriteLine("deltaX:" + deltaX);
                double deltaX2 = deltaX > 0 ? hDelta : -hDelta;
                double x2 = DetailScrollViewerTranslateY.TranslateX + deltaX2;
                if (Math.Abs(x2) <= hMax)
                {
                    DetailScrollViewerTranslateY.TranslateX = x2;
                    if (x2 > 0)
                    {
                        PrevLineTransform.ScaleX = (float)(32f / hMax) * x2;
                        if (PrevLineTransform.ScaleX == 32)
                        {
                            isBeginAction = true;
                            Left(sender, new RoutedEventArgs());
                        }
                    } else
                    {
                        NextLineTransform.ScaleX = (float)(34f / hMax) * Math.Abs(x2);
                        if (NextLineTransform.ScaleX == 34)
                        {
                            isBeginAction = true;
                            Right(sender, new RoutedEventArgs());
                        }
                    }
                }
            }

            prevPoint = e.Position;
        }

        // завершение
        DispatcherTimer dispatcherTimer;
        private void DetailScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                return;
            }

            if (DetailScrollViewerTranslateY.TranslateX != 0)
            {
                if (!isBeginAction)
                {
                    DetailScrollViewerTranslateXToZero.Begin();
                }
            }

            isVertical = null;
            //System.Diagnostics.Debug.WriteLine("lastDelta:" + lastDelta);

            dispatcherTimer.Start();
            //lastDelta = 0;
        }

        // завершаем таймер
        private void TimerEnd()
        {
            slow = 0.1;
            dispatcherTimer.Stop();
            if (DetailScrollViewerTranslateY.TranslateY != 0)
            {
                if (!isBeginAction)
                {
                    DetailScrollViewerTranslateYToZero.Begin();
                }
            }
        }

        // докатываемся
        private void DispatcherTimer_Tick(object sender, object e)
        {
            lastDelta += lastDelta > 0 ? -0.3 : 0.3;
            if ((lastDelta <= 1) && (-1 <= lastDelta))
            {
                TimerEnd();
                return;
            }

            double y = DetailStackPanelTranslateY.TranslateY + lastDelta;
            if (slow < 1)
            {
                slow += 0.1;
            }
            if ((y < 0) && (y > DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight) && (DetailScrollViewerTranslateY.TranslateY == 0))
            {
                DetailStackPanelTranslateY.TranslateY = y;
            } else
            {
                TimerEnd();
            }
        }

        private void DetailScrollViewerTranslateYToZero_Completed(object sender, object e)
        {
            DetailScrollViewerTranslateY.TranslateY = 0;
        }

        private void DetailScrollViewerTranslateXToZero_Completed(object sender, object e)
        {
            DetailScrollViewerTranslateY.TranslateX = 0;
        }
    }
}
