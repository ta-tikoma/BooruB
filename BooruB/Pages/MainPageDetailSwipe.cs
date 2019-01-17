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
        bool? isVertical = null;
        bool isBeginLeftOrRight = false;

        private void DetailScrollViewer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            prevPoint = e.Position;
            DetailScrollViewerTranslateYToZero.Stop();
            dispatcherTimer.Stop();
            isBeginLeftOrRight = false;
        }

        double slow = 0.1;
        double maxDelta = 0;
        private void DetailScrollViewer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (isVertical == null)
            {
                isVertical = Math.Abs(e.Position.Y - prevPoint.Y) > Math.Abs(e.Position.X - prevPoint.X);
            }

            if (isVertical == true)
            {
                double deltaY = ((int) e.Position.Y - (int) prevPoint.Y) * slow;
                maxDelta = deltaY;

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
                                dispatcherTimer.Stop();
                                DetailClose(null, new RoutedEventArgs());
                            }
                        }
                    }
                }
            }

            if (isVertical == false)
            {
                double deltaX = (int)e.Position.X - (int)prevPoint.X;
                if (deltaX == 0)
                {
                    return;
                }

                //System.Diagnostics.Debug.WriteLine("deltaX:" + deltaX);
                double deltaX2 = deltaX > 0 ? 1 : -1;
                double x2 = DetailScrollViewerTranslateY.TranslateX + deltaX2;
                if (Math.Abs(x2) <= 42)
                {
                    DetailScrollViewerTranslateY.TranslateX = x2;
                    if (x2 > 0)
                    {
                        PrevLineTransform.ScaleX = (float)(32f / 42f) * x2;
                        if (PrevLineTransform.ScaleX == 32)
                        {
                            isBeginLeftOrRight = true;
                            Left(sender, new RoutedEventArgs());
                        }
                    } else
                    {
                        NextLineTransform.ScaleX = (float)(34f / 42f) * Math.Abs(x2);
                        if (NextLineTransform.ScaleX == 34)
                        {
                            isBeginLeftOrRight = true;
                            Right(sender, new RoutedEventArgs());
                        }
                    }
                }
            }
            prevPoint = e.Position;
        }

        DispatcherTimer dispatcherTimer;
        private void DetailScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (DetailScrollViewerTranslateY.TranslateX != 0)
            {
                if (!isBeginLeftOrRight)
                {
                    DetailScrollViewerTranslateXToZero.Begin();
                }
            }

            isVertical = null;
            //System.Diagnostics.Debug.WriteLine("maxDelta:" + maxDelta);

            dispatcherTimer.Start();
            //maxDelta = 0;
        }

        private void TimerEnd()
        {
            slow = 0.1;
            dispatcherTimer.Stop();
            if (DetailScrollViewerTranslateY.TranslateY != 0)
            {
                DetailScrollViewerTranslateYToZero.Begin();
            }
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            maxDelta += maxDelta > 0 ? -0.6 : 0.6;
            if ((maxDelta <= 1) && (-1 <= maxDelta))
            {
                TimerEnd();
                return;
            }

            double y = DetailStackPanelTranslateY.TranslateY + maxDelta;
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
