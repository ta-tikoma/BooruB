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
        // поиск анимация показа\скрытия
        private void ShowSearchPanel(object sender, RoutedEventArgs e)
        {
            if (Search.Visibility == Visibility.Collapsed)
            {
                Search.Visibility = Visibility.Visible;
                ShowSearch.Begin();
            }
            else
            {
                HideSearch.Begin();
            }
        }

        private void HideSearchIconComplited(object sender, object e)
        {
            FindIcon.Symbol = Symbol.Cancel;
            ShowSearch2.Begin();
        }

        private void HideCanselIconComplited(object sender, object e)
        {
            FindIcon.Symbol = Symbol.Find;
            HideSearch2.Begin();
        }

        private void SearchBorderHideCompleted(object sender, object e)
        {
            Search.Visibility = Visibility.Collapsed;
        }
        // поиск анимация показа\скрытия

        // поле тега
        private void TagClick(object sender, RoutedEventArgs e)
        {
            menuTag = (sender as Button).DataContext as Models.Tag;
            OpenTag_Click(sender, e);
        }

        private void SearchInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchInputOpenTag(sender, new RoutedEventArgs());
            }
        }

        public void SearchInputAddTag(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = SearchInput.Text,
                Code = SearchInput.Text.ToLower(),
                SiteUrl = App.Settings.current_site
            };
            SearchInput.Text = "";
            AddToTags_Click(sender, e);
        }

        private void SearchInputOpenTag(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = SearchInput.Text,
                Code = SearchInput.Text.ToLower(),
                SiteUrl = App.Settings.current_site
            };
            SearchInput.Text = "";
            OpenTag_Click(sender, e);
        }
        // поле тега

        // теги меню
        Models.Tag menuTag = null;
        private void TagHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (tagMenuIsOpen)
            {
                return;
            }
            Button button = sender as Button;
            menuTag = button.DataContext as Models.Tag;

            if (HistoryOfTags.HasTag(menuTag))
            {
                MenuFlyoutSeparator.Visibility = Visibility.Visible;
                RemoveFromHistory.Visibility = Visibility.Visible;
            }
            else
            {
                MenuFlyoutSeparator.Visibility = Visibility.Collapsed;
                RemoveFromHistory.Visibility = Visibility.Collapsed;
            }

            if (App.Settings.current_site != menuTag.SiteUrl)
            {
                OpenTagOnThisSIte.Visibility = Visibility.Visible;
            }
            else
            {
                OpenTagOnThisSIte.Visibility = Visibility.Collapsed;
            }

            string[] tags = new string[] { };
            if (App.Settings.current_tag_code != null)
            {
                tags = App.Settings.current_tag_code.Split(new char[] { '+' }).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }

            if (tags.Contains(menuTag.Code))
            {
                OpenTag.Visibility = Visibility.Collapsed;
                AddToTags.Visibility = Visibility.Collapsed;
                RemoveFromTags.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (string code in tags)
                {
                    System.Diagnostics.Debug.WriteLine("code_" + code);
                }
                if (tags.Count() == 0)
                {
                    AddToTags.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AddToTags.Visibility = Visibility.Visible;
                }
                RemoveFromTags.Visibility = Visibility.Collapsed;
            }

            TagMenu.ShowAt(button, e.GetPosition(button));
        }

        bool tagMenuIsOpen = false;
        private void TagMenu_Opened(object sender, object e)
        {
            tagMenuIsOpen = true;
        }

        private void TagMenu_Closed(object sender, object e)
        {
            tagMenuIsOpen = false;
        }
        // теги меню

        // теги действия
        private void HidePanels()
        {
            if (Detail.DataContext != null)
            {
                HideDetail.Begin();
                Detail.DataContext = null;
            }

            if (Search.Visibility == Visibility.Visible)
            {
                HideSearch.Begin();
            }
        }

        private void OpenTagOnThisSIte_Click(object sender, RoutedEventArgs e)
        {
            menuTag = new Models.Tag()
            {
                Name = menuTag.Name,
                Code = menuTag.Code,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void OpenTag_Click(object sender, RoutedEventArgs e)
        {
            HidePanels();
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code + ":");
            System.Diagnostics.Debug.WriteLine("menuTag.SiteUrl:" + menuTag.SiteUrl + ":");
            if (menuTag.Code == "")
            {
                App.Settings.current_tag_code = null;
            }
            else
            {
                HistoryOfTags.AddTag(menuTag);
                App.Settings.current_site = menuTag.SiteUrl;
                App.Settings.current_tag_code = menuTag.Code;
            }
            Images.ClearSelf();
            menuTag = null;
            HistoryOfTags.ChaneCurrentTag();
        }

        private void AddToTags_Click(object sender, RoutedEventArgs e)
        {
            List<string> tags = new List<string>();
            if (App.Settings.current_tag_code != null)
            {
                tags = App.Settings.current_tag_code.Split(new char[] { '+' }).ToList();
            }

            System.Diagnostics.Debug.WriteLine("App.Settings.current_tag_code:" + App.Settings.current_tag_code);

            tags.Add(menuTag.Code);
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code);

            string newTagCode = String.Join("+", tags.OrderBy(x => x).Distinct().ToArray());
            System.Diagnostics.Debug.WriteLine("newTagCode:" + newTagCode);

            menuTag = new Models.Tag()
            {
                Name = newTagCode,
                Code = newTagCode,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void RemoveFromTags_Click(object sender, RoutedEventArgs e)
        {
            List<string> tags = App.Settings.current_tag_code.Split(new char[] { '+' }).ToList();
            tags.Remove(menuTag.Code);
            System.Diagnostics.Debug.WriteLine("menuTag.Code:" + menuTag.Code);

            string newTagCode = String.Join("+", tags.OrderBy(x => x).Distinct().ToArray());
            System.Diagnostics.Debug.WriteLine("newTagCode:" + newTagCode);

            menuTag = new Models.Tag()
            {
                Name = newTagCode,
                Code = newTagCode,
                SiteUrl = App.Settings.current_site
            };

            OpenTag_Click(sender, e);
        }

        private void RemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryOfTags.RemoveTag(menuTag);
            menuTag = null;
        }
        // теги действия
    }
}
