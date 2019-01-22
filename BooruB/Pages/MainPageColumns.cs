﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("ImagesScrollViewer.VerticalOffset - ((Frame)Window.Current.Content).ActualHeight:" + (ImagesScrollViewer.ScrollableHeight - ((Frame)Window.Current.Content).ActualHeight));
            //System.Diagnostics.Debug.WriteLine("VerticalOffset:" + ImagesScrollViewer.VerticalOffset);
            //System.Diagnostics.Debug.WriteLine("ScrollableHeight:" + ImagesScrollViewer.ScrollableHeight);
            //if (ImagesScrollViewer.VerticalOffset == ImagesScrollViewer.ScrollableHeight)
            //{
            ImagesGrid.CheckVisible(ImagesScrollViewer.VerticalOffset);
            //}
        }

        private void ImagesScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("ImagesScrollViewer_Loaded:");
            ImagesGrid.ImagesLoad();
        }
    }
}
