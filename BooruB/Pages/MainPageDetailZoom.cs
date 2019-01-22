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

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
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
    }
}
