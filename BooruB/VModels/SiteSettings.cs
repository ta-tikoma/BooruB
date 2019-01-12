using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace BooruB.VModels
{
    class SiteSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Models.Site site = null;

        public SiteSettings(Models.Site site)
        {
            this.site = site;
        }

        public string Name
        {
            get { return site.Name; }
        }

        public bool UseProxy
        {
            get { return site.UseProxy; }
            set
            {
                site.UseProxy = value;
                System.Diagnostics.Debug.WriteLine("UseProxy:" + value);
                Save();
                RaiseProperty("UseProxy");
            }
        }

        private void Save()
        {
            ObservableCollection<Models.Site> Sites = Models.Site.Load();
            for (int i = 0; i < Sites.Count; i++)
            {
                if (Sites[i].Name == site.Name)
                {
                    Sites[i] = site;
                }
            }
            Models.Site.Save(Sites);

        }

        public Visibility _ProxySelectorVisibility = Visibility.Collapsed;
        public Visibility ProxySelectorVisibility
        {
            get { return _ProxySelectorVisibility; }
            set { _ProxySelectorVisibility = value; RaiseProperty("ProxySelectorVisibility"); }
        }

        public static void Save(ObservableCollection<SiteSettings> list)
        {
            ObservableCollection<Models.Site> _list = new ObservableCollection<Models.Site>();
            foreach (SiteSettings site in list)
            {
                _list.Add(site.site);
            }
            Models.Site.Save(_list);
        }

        public static ObservableCollection<VModels.SiteSettings> Load()
        {
            ObservableCollection<SiteSettings> Sites = new ObservableCollection<SiteSettings>();
            foreach (Models.Site site in Models.Site.Load())
            {
                Sites.Add(new VModels.SiteSettings(site));
            }

            return Sites;
        }
    }
}
