using BooruB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BooruB.Models
{
    public sealed class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            //System.Diagnostics.Debug.WriteLine("save:settings_" + name);
            ApplicationData.Current.LocalSettings.Values["settings_" + name] = this.GetType().GetProperty(name).GetValue(this);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /*
        public int _zoom_side_size { get; set; } = 100;
        public int zoom_side_size
        {
            get { return _zoom_side_size; }
            set { _zoom_side_size = value; RaiseProperty("zoom_side_size"); }
        }
        */

        public int _images_in_row { get; set; } = 3;
        public int images_in_row
        {
            get { return _images_in_row; }
            set { _images_in_row = value; RaiseProperty("images_in_row"); }
        }

        public int _max_detail_width { get; set; } = 700;
        public int max_detail_width
        {
            get { return _max_detail_width; }
            set { _max_detail_width = value; RaiseProperty("max_detail_width"); }
        }

        public int _max_side_size { get; set; } = -1;
        public int max_side_size
        {
            get { return _max_side_size; }
            set { _max_side_size = value; RaiseProperty("max_side_size"); }
        }

        public int _side_size { get; set; } = 100;
        public int side_size
        {
            get { return _side_size; }
            set { _side_size = value; RaiseProperty("side_size"); }
        }

        public int _columns_count { get; set; } = 3;
        public int columns_count
        {
            get { return _columns_count; }
            set { _columns_count = value; RaiseProperty("columns_count"); }
        }

        public string _proxy { get; set; } = "";
        public string proxy
        {
            get { return _proxy; }
            set { _proxy = value; RaiseProperty("proxy"); }
        }

        // текущий сайт
        public string _current_tag_code { get; set; } = "";
        public string current_tag_code
        {
            get { return _current_tag_code; }
            set { _current_tag_code = value; RaiseProperty("current_tag_code"); }
        }

        public string _current_site { get; set; } = "";
        public string current_site
        {
            get { return _current_site; }
            set {
                _current_site = value;
                RaiseProperty("current_site");
                foreach (Windows.UI.Xaml.Controls.AppBarButton appBarButton in Pages.MainPage.AppBarButtons)
                {
                    System.Diagnostics.Debug.WriteLine("appBarButton.Tag:" + appBarButton.Tag);
                    if (appBarButton.Tag.ToString() == current_site)
                    {
                        appBarButton.Icon = new Windows.UI.Xaml.Controls.SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Accept);
                    } else
                    {
                        appBarButton.Icon = null;
                    }
                }
                
                /*foreach (VModels.SiteMainPage _site in Sites)
                {
                    if (current_site == _site.site.Url)
                    {
                        System.Diagnostics.Debug.WriteLine("current_site:" + current_site);
                        _site.Icon = new Windows.UI.Xaml.Controls.SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Accept);
                    } else
                    {
                        _site.Icon = null;
                    }
                }*/
            }
        }

        private Site site = null;

        public Query Query = new Query();

        public Site GetSite()
        {
            FoundSite();
            return site;
        }

        private void FoundSite()
        {
            System.Diagnostics.Debug.WriteLine("FoundSite");

            if (site == null)
            {
                ObservableCollection<Site> sites = Site.Load();
                foreach (Site _site in sites)
                {
                    if (_site.Url == current_site)
                    {
                        System.Diagnostics.Debug.WriteLine("_site:" + _site.Url);
                        site = _site;
                        if (site.UseProxy)
                        {
                            Query = new QueryProxy();
                        } else
                        {
                            Query = new Query();
                        }
                        return;
                    }
                }
                if (sites.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("sites.First():" + sites.First().Url);
                    site = sites.First();
                    current_site = site.Url;
                    if (site.UseProxy)
                    {
                        Query = new QueryProxy();
                    }
                    else
                    {
                        Query = new Query();
                    }
                }
            }
        }

        public string GetCurrentLink()
        {
            // если текущий сайт не найден
            FoundSite();

            if (site == null)
            {
                return null;
            }

            return site.Url;
        }

        public string GetListLink()
        {
            System.Diagnostics.Debug.WriteLine("current_site:" + current_site);

            FoundSite();

            // если текущий сайт не найден
            if (site == null)
            {
                System.Diagnostics.Debug.WriteLine("(site == null):1");
                return null;
            }

            // если изменился сайт
            if (current_site != site.Url)
            {
                System.Diagnostics.Debug.WriteLine("(current_site != site.Url)");
                site = null;
                FoundSite();
            }

            // если текущий сайт не найден
            if (site == null)
            {
                System.Diagnostics.Debug.WriteLine("(site == null):2");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("site.Url:" + site.Url);
            string link = site.Url + "index.php?page=post&s=list&pid=0";
            if ((current_tag_code != null) && (current_tag_code.Length != 0))
            {
                link += "&tags=" + current_tag_code;
            }

            System.Diagnostics.Debug.WriteLine("link:" + link);

            // если без прокси
            return link;
        }
        // текущий сайт

        public string _image_save_path { get; set; } = "";
        public string image_save_path
        {
            get { return _image_save_path; }
            set { _image_save_path = value; RaiseProperty("image_save_path"); }
        }

        public Settings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.Name[0] == '_')
                {
                    if (localSettings.Values.Keys.Contains("settings" + property.Name))
                    {
                        try
                        {
                            property.SetValue(this, localSettings.Values["settings" + property.Name]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
    }
}
