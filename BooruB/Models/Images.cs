using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace BooruB.Models
{
    class Images : ObservableCollection<Image>, ISupportIncrementalLoading
    {
        const int FIRST_PAGE = 1;

        private int page = FIRST_PAGE;
        private int number = 1;
        private string next_page_link = "";
        private bool has_more_items = true;
        private bool busy = false;

        public async Task<uint> TLoad()
        {
            uint count = 0;

            if (next_page_link == "")
            {
                next_page_link = App.Settings.GetListLink();
            } else if (next_page_link.Substring(0, 4) != "http")
            {
                next_page_link = App.Settings.GetCurrentLink() + next_page_link;
            }

            System.Diagnostics.Debug.WriteLine("link:" + next_page_link);
            string response = await App.Settings.Query.Get(next_page_link);
            if (response == null)
            {
                Pages.MainPage.SetNothingFoundText("ENABLE INTERNET.");
            }
            else
            {
                Pages.MainPage.SetNothingFoundText("NOTHING FOUND.");
                //Helpers.LoadingAnimation.SetPage(page);

                // изображения
                //Regex regex = new Regex("<a[^>]*href=\"(?<detail>[^\"]+id=[^\"]+)\"[^>]*>.*<img[^>]*src=\"(?<thumbnail>[^\"]+)\"[^>]*/>.*</a>");
                Regex regex = new Regex("<a[^>]*href=\"(?<detail>[^\"]+id=[^\"]+)\"[^>]*><img[^>]*src=\"(?<thumbnail>[^\"]+)\"[^>]*/></a>");
                MatchCollection matches = regex.Matches(response);
                foreach (Match match in matches)
                {
                    string DetaillUrl = null;
                    string ThumbnailUrl = null;
                    GroupCollection collection = match.Groups;
                    for (int i = 0; i < collection.Count; i++)
                    {
                        Group group = collection[i];
                        if (regex.GroupNameFromNumber(i) == "detail")
                        {
                            DetaillUrl = group.Value.Trim().Replace("&amp;", "&");
                        }
                        if (regex.GroupNameFromNumber(i) == "thumbnail")
                        {
                            ThumbnailUrl = group.Value.Trim();
                        }
                    }

                    Models.Image image = null;

                    if ((DetaillUrl != null) && (ThumbnailUrl != null))
                    {
                        //System.Diagnostics.Debug.WriteLine("DetaillUrl:" + DetaillUrl);
                        //System.Diagnostics.Debug.WriteLine("ThumbnailUrl:" + ThumbnailUrl);
                        image = new Models.Image()
                        {
                            DetaillPageUrl = DetaillUrl,
                            ThumbnailUrl = ThumbnailUrl,
                            Number = number,
                            Page = page,
                            NextPageLink = next_page_link
                        };
                        number++;
                    }

                    if (image != null)
                    {
                        count++;
                        this.Add(image);
                    }
                }

                // следующая страница
                string _next_page_link = "";
                page++;
                try
                {
                    Regex regexNP = new Regex("<a[^>]*href=\"([^\"]+)\"[^>]*>" + page + "</a>");
                    Match matchNP = regexNP.Match(response);
                    _next_page_link = matchNP.Groups[1].Value;
                }
                catch (Exception)
                {
                }

                if (_next_page_link == "")
                {
                    has_more_items = false;
                } else
                {
                    next_page_link = _next_page_link.Replace("&amp;", "&");
                }
            }

            // показ плашки NOT FOUND
            if (Items.Count == 0)
            {
                Pages.MainPage.ShowNothingFound();
                has_more_items = false;
            }
            else
            {
                Pages.MainPage.HideNothingFound();
            }

            busy = false;
            return count;
        }

        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            busy = true;
            Pages.MainPage.ShowListLoading();
            try
            {
                count = await TLoad();
                return new LoadMoreItemsResult { Count = (uint)count };
            }
            finally
            {
                busy = false;
                Pages.MainPage.HideListLoading();
            }
        }

        public async void ClearSelf(Models.Page page = null)
        {
            number = 1;
            has_more_items = true;

            if (page == null)
            {
                this.page = FIRST_PAGE;
                next_page_link = "";
            } else
            {
                this.page = page.Number;
                next_page_link = page.NextPageLink;
            }

            busy = false;
            Page.Save(this.page, next_page_link);
            this.Clear();
            //if (Pages.MainPage.IsNothingFound())
            {
                await AsyncInfo.Run((c) => LoadMoreItemsAsync(c, 0));
                //await AsyncInfo.Run((c) => LoadMoreItemsAsync(c, 0));
            }
        }

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count)
        {
            if (busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        bool ISupportIncrementalLoading.HasMoreItems
        {
            get { return has_more_items; }
        }
    }
}
