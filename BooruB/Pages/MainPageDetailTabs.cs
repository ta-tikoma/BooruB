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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        double nextYPosition = 0;
        private void TabAnimationScroll_Completed(object sender, object e)
        {
            SwipeTextPanel.Height = DetailScrollViewer.ActualHeight;
            DetailStackPanelTranslateY.TranslateY = nextYPosition;
            isTabAnimationRun = false;
        }

        private void TabAnimation_Completed(object sender, object e)
        {
            SwipeTextPanel.Height = DetailScrollViewer.ActualHeight;
            //DetailStackPanelTranslateY.TranslateY = DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight - 10;
            nextYPosition = DetailScrollViewer.ActualHeight - DetailStackPanel.ActualHeight;
            if (nextYPosition > 0)
            {
                nextYPosition = 0;
            }
            TabAnimationTranslateY.To = nextYPosition;
            //DetailScrollViewer.ActualHeight - (DetailStackPanel.ActualHeight - CurrentTab.ActualHeight + to.ActualHeight) - 10;
            TabAnimationScroll.Begin();
        }

        ListView CurrentTab = null;
        bool isTabAnimationRun = false;

        private void SetAnimation(ListView to)
        {
            if (CurrentTab == to)
            {
                return;
            }

            isTabAnimationRun = true;
            TabAnimationSize.To = to.ActualHeight + 10;
            CurrentTab.Opacity = 0;
            CurrentTab.Margin = new Thickness(0, to.ActualHeight + 20, 0, 0);
            to.Margin = new Thickness(0);
            to.Opacity = 1;
            CurrentTab = to;
            TabAnimation.Begin();
        }

        private void CurrentTab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabContainers.Height = CurrentTab.ActualHeight;
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (isTabAnimationRun)
            {
                return;
            }

            switch ((sender as Button)?.Tag.ToString())
            {
                case "TAGS":
                    SetAnimation(TagsContainer);
                    TagsButton.Foreground = (SolidColorBrush) Resources["ButtonForegroundThemeBrush"];
                    StatisticButton.Foreground = (SolidColorBrush) Resources["AppBarItemDisabledForegroundThemeBrush"];
                    CommentsButton.Foreground = (SolidColorBrush)Resources["AppBarItemDisabledForegroundThemeBrush"];
                    break;
                case "STATISTIC":
                    SetAnimation(StatisticContainer);
                    TagsButton.Foreground = (SolidColorBrush)Resources["AppBarItemDisabledForegroundThemeBrush"];
                    StatisticButton.Foreground = (SolidColorBrush)Resources["ButtonForegroundThemeBrush"];
                    CommentsButton.Foreground = (SolidColorBrush)Resources["AppBarItemDisabledForegroundThemeBrush"];
                    break;
                case "COMMENTS":
                    SetAnimation(CommentsContainer);
                    TagsButton.Foreground = (SolidColorBrush)Resources["AppBarItemDisabledForegroundThemeBrush"];
                    StatisticButton.Foreground = (SolidColorBrush)Resources["AppBarItemDisabledForegroundThemeBrush"];
                    CommentsButton.Foreground = (SolidColorBrush)Resources["ButtonForegroundThemeBrush"];
                    break;
            }
        }
    }
}
