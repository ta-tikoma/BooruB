using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace BooruB.Models
{
    class Page
    {
        public int Number { get; set; } = 0;
        public string NextPageLink { get; set; } = "";

        public static Page GetCurrentPage()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string key = "page_" + GetHash(App.Settings.current_site + App.Settings.current_tag_code);
            if (localSettings.Values.ContainsKey(key))
            {
                return Load(localSettings.Values[key].ToString());
            }
            return null;
        }

        public static Page Load(string arg)
        {
            JsonObject jsonObject = JsonObject.Parse(arg);

            return new Page()
            {
                Number = (int)jsonObject.GetNamedNumber("Number"),
                NextPageLink = jsonObject.GetNamedString("NextPageLink")
            };
        }

        public static void Save(int number, string next_page_link)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string key = "page_" + GetHash(App.Settings.current_site + App.Settings.current_tag_code);
            System.Diagnostics.Debug.WriteLine("key:" + key);
            if (localSettings.Values.ContainsKey(key))
            {
                Page page = Load(localSettings.Values[key].ToString());
                if (page.Number >= number)
                {
                    return;
                }
            }

            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue("Number", JsonValue.CreateNumberValue(number));
            jsonObject.SetNamedValue("NextPageLink", JsonValue.CreateStringValue(next_page_link));
            localSettings.Values[key] = jsonObject.ToString();
        }

        public static void Remove(string site_url, string tag_code)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string key = "page_" + GetHash(App.Settings.current_site + App.Settings.current_tag_code);
            if (localSettings.Values.ContainsKey(key))
            {
                localSettings.Values.Remove(key);
            }
        }

        public static string GetHash(string value)
        {
            using (HashAlgorithm algorithm = MD5.Create())
            {
                value = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
            return value;
        }
    }
}
