using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace BooruB.Models
{
    public class Site
    {

        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public bool UseProxy { get; set; } = false;

        public Site()
        {
        }

        public void RemoveTags()
        {
            List<Tag> tags = Tag.Load();
            foreach (Tag tag in tags)
            {
                if (tag.SiteUrl == Url)
                {
                    tag.RemovePage();
                    tags.Remove(tag);
                }
            }
            Tag.Save(tags);
        }

        public void RemovePage()
        {
            Models.Page.Remove(Url, "");
        }

        public Site(string arg)
        {
            JsonObject jsonObject = JsonObject.Parse(arg);
            Name = jsonObject.GetNamedString("Name");
            Url = jsonObject.GetNamedString("Url");
            if (jsonObject.ContainsKey("UseProxy"))
            {
                UseProxy = jsonObject.GetNamedBoolean("UseProxy");
            }
        }

        public new string ToString()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue("Name", JsonValue.CreateStringValue(Name));
            jsonObject.SetNamedValue("Url", JsonValue.CreateStringValue(Url));
            jsonObject.SetNamedValue("UseProxy", JsonValue.CreateBooleanValue(UseProxy));
            return jsonObject.ToString();
        }

        public static ObservableCollection<Site> Load()
        {
            ObservableCollection<Site> list = new ObservableCollection<Site>();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;
            while (localSettings.Values.ContainsKey("sites_" + index))
            {
                list.Add(new Site(localSettings.Values["sites_" + index].ToString()));
                index++;
            }

            return list;
        }

        public static void Save(ObservableCollection<Site> list)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int i = 0;
            for (; i < list.Count(); i++)
            {
                localSettings.Values["sites_" + i] = list[i].ToString();
            }
            while (localSettings.Values.ContainsKey("sites_" + i))
            {
                localSettings.Values.Remove("sites_" + i);
                i++;
            }
        }
    }
}
