﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;

namespace BooruB.Models
{
    class Comment
    {
        public string Author { get; set; } = null;
        public string Date { get; set; } = null;
        public string Text { get; set; } = null;
    }

    class Statistic
    {
        public string Name { get; set; } = null;
        public string Value { get; set; } = null;
    }

    class Image
    {
        public int Page { get; set; } = 1;
        public string NextPageLink { get; set; } = null;
        public int Number { get; set; } = 1;
        public string ThumbnailUrl { get; set; } = null;

        public string Id { get; set; } = "";
        public string DetaillPageUrl { get; set; } = null;

        public bool DetailIsLoad { get; set; } = false;
        public string DetailImageUrl { get; set; } = null;
        public List<Tag> Tags { get; set; } = null;
        public List<Statistic> Statistics { get; set; } = null;
        public ObservableCollection<Comment> Comments { get; set; } = new ObservableCollection<Comment>();

        public async Task<Image> DetailDataReLoad()
        {
            DetailIsLoad = false;

            try
            {
                string type = Models.Image.GetType(DetailImageUrl);
                string hash = Models.Page.GetHash(DetailImageUrl);
                StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(hash + type);
                await tempFile.DeleteAsync();
            }
            catch (Exception)
            {
            }

            return await DetailDataLoad();
        }

        public async Task<Image> DetailDataLoad()
        {
            if (DetailIsLoad)
            {
                return this;
            }

            Pages.MainPage.ShowListLoading();

            string response = await App.Settings.Query.Get(App.Settings.GetCurrentLink() + DetaillPageUrl);
            //System.Diagnostics.Debug.WriteLine("DetaillPageUrl:" + App.Settings.current_site + DetaillPageUrl);

            try
            {
                // ссылка на изображение
                Regex regexDIU = new Regex("<img[^>]*src=\"([^\"]+)\"[^>]*id=\"image\"[^>]*/>");
                Match matchDIU = regexDIU.Match(response);
                DetailImageUrl = matchDIU.Groups[1].Value;
                if (DetaillPageUrl == null)
                {
                    Pages.MainPage.HideListLoading();
                    return null;
                }
                if (DetaillPageUrl.Length == 0)
                {
                    Pages.MainPage.HideListLoading();
                    return null;
                }
                //System.Diagnostics.Debug.WriteLine("matchDIU:" + matchDIU);
                //System.Diagnostics.Debug.WriteLine("DetailImageUrl:" + DetailImageUrl);

                // теги
                Tags = new List<Tag>();
                Regex regexT = new Regex("<a[^>]*href=\"[^\"]+tags=(?<code>[a-z0-9-_]+)[^\"]*\"[^>]*>(?<name>[^<]+)</a>([ ]*)(|<span[^>]+>)(?<count>[0-9]+)");
                //Regex regexT = new Regex("<a[^>]*href=\"[^\"]+tags=(?<code>[a-z0-9-_]+)[^\"]*\"[^>]*>(?<name>[^<]+)</a>([ ]*)(?<count>[0-9]+)");
                MatchCollection matchesT = regexT.Matches(response);
                foreach (Match match in matchesT)
                {
                    string NameT = null;
                    string CodeT = null;
                    string CountT = null;
                    GroupCollection collectionT = match.Groups;
                    for (int i = 0; i < collectionT.Count; i++)
                    {
                        Group groupT = collectionT[i];
                        if (regexT.GroupNameFromNumber(i) == "name")
                        {
                            NameT = groupT.Value.Trim();
                        }
                        if (regexT.GroupNameFromNumber(i) == "code")
                        {
                            CodeT = groupT.Value.Trim().Replace("&amp;", "&");
                        }
                        if (regexT.GroupNameFromNumber(i) == "count")
                        {
                            CountT = groupT.Value.Trim().Replace("&amp;", "&");
                        }
                    }

                    if ((NameT != null) && (CodeT != null))
                    {
                        if ((CodeT != "all"))
                        {
                            Tags.Add(new Tag()
                            {
                                Name = NameT,
                                Code = CodeT,
                                Count = CountT,
                                SiteUrl = App.Settings.current_site
                            });
                        }
                    }
                }

                // статистика
                Statistics = new List<Statistic>();
                Regex regexS = new Regex("(?<name>Id|Posted|Size|Source|Score|By|Rating):(?<value>[^<]+)<");
                MatchCollection matchesS = regexS.Matches(response);
                foreach (Match match in matchesS)
                {
                    string NameS = null;
                    string ValueS = null;
                    GroupCollection collectionS = match.Groups;
                    for (int i = 0; i < collectionS.Count; i++)
                    {
                        Group groupS = collectionS[i];
                        if (regexS.GroupNameFromNumber(i) == "name")
                        {
                            NameS = groupS.Value.Trim();
                        }
                        if (regexS.GroupNameFromNumber(i) == "value")
                        {
                            ValueS = groupS.Value.Trim().Replace("&amp;", "&");
                        }
                    }

                    if ((NameS != null) && (ValueS != null))
                    {
                        if ((NameS.Length > 0) && (ValueS.Length > 0))
                        {
                            Statistics.Add(new Statistic()
                            {
                                Name = NameS,
                                Value = ValueS
                            });
                        }
                    }
                }

                // комментарии
                LoadComments(response);

                DetailIsLoad = true;
            }
            catch (Exception)
            {
                Pages.MainPage.HideListLoading();
                return null;
            }

            Pages.MainPage.HideListLoading();
            return this;
        }

        public void LoadComments(string response)
        {
            Comments.Clear();
            LoadComments0_1_11(response);
            if (Comments.Count == 0)
            {
                LoadComments0_2(response);
            }
        }

        public void LoadComments0_2(string response)
        {
            //System.Diagnostics.Debug.WriteLine("LoadComments0_2:" + response);
            Regex regexC = new Regex(@"<div[^>]+><br /><a[^>]+>(?<author>[^<]+)</a> <span[^>]+>[^<]+</span><br /><b>Posted on (?<date>[0-9-: ]+) Score: <a[^>]+>[^<]+</a> \u0028vote <a[^>]+>Up</a>/<a[^>]+>Down</a>\u0029&nbsp;&nbsp;&nbsp;\u0028<a[^>]+></a><a[^>]+>Report as spam</a>\u0029</b><br />(?<text>.*)<br /></div>");
            MatchCollection matchesC = regexC.Matches(response);
            foreach (Match match in matchesC)
            {
                string AuthorC = null;
                string DateC = null;
                string TextC = null;
                //System.Diagnostics.Debug.WriteLine("match.Value:" + match.Value);
                GroupCollection collectionC = match.Groups;
                for (int i = 0; i < collectionC.Count; i++)
                {
                    Group groupC = collectionC[i];
                    //System.Diagnostics.Debug.WriteLine("groupC.Value:" + groupC.Value);
                    if (regexC.GroupNameFromNumber(i) == "author")
                    {
                        AuthorC = groupC.Value.Trim();
                    }
                    if (regexC.GroupNameFromNumber(i) == "date")
                    {
                        DateC = groupC.Value.Trim();
                    }
                    if (regexC.GroupNameFromNumber(i) == "text")
                    {
                        TextC = Regex.Replace(groupC.Value.Trim(), @"<a[^>]*>|</a>", "");
                    }
                }

                if ((AuthorC != null) && (DateC != null) && (TextC != null))
                {
                    Comments.Add(new Comment()
                    {
                        Author = AuthorC,
                        Text = TextC,
                        Date = DateC
                    });
                }
            }
        }

        public void LoadComments0_1_11(string response)
        {
            Regex regexC = new Regex("<a[^>]+>(?<author>[^<]+)</a><br /><b>Posted on (?<date>[0-9-: ]+).*</b><br />(?<text>.*)<br /></div>");
            MatchCollection matchesC = regexC.Matches(response);
            foreach (Match match in matchesC)
            {
                string AuthorC = null;
                string DateC = null;
                string TextC = null;
                GroupCollection collectionC = match.Groups;
                for (int i = 0; i < collectionC.Count; i++)
                {
                    Group groupC = collectionC[i];
                    if (regexC.GroupNameFromNumber(i) == "author")
                    {
                        AuthorC = groupC.Value.Trim();
                    }
                    if (regexC.GroupNameFromNumber(i) == "date")
                    {
                        DateC = groupC.Value.Trim();
                    }
                    if (regexC.GroupNameFromNumber(i) == "text")
                    {
                        TextC = Regex.Replace(groupC.Value.Trim(), @"<a[^>]*>|</a>", "");
                    }
                }

                if ((AuthorC != null) && (DateC != null) && (TextC != null))
                {
                    Comments.Add(new Comment()
                    {
                        Author = AuthorC,
                        Text = TextC,
                        Date = DateC
                    });
                }
            }
        }

        public static string GetType(string url)
        {
            foreach (string type in new[] { ".jpeg", ".jpg", ".gif", ".bmp" })
            {
                if (url.IndexOf(type) != -1)
                {
                    return type;
                }
            }
            return ".png";
        }
    }
}
