using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BooruB.Models
{
    class TagGroups : ObservableCollection<TagGroup>
    {
        public static TagGroups Load()
        {
            TagGroups tagGroups = new TagGroups();

            foreach (Tag tag in Tag.Load())
            {
                tagGroups.GroupBySite(tag.Site).Tags.Add(tag);
            }
            return tagGroups;
        }

        public TagGroup GroupBySite(string site)
        {
            foreach (TagGroup tagGroup in Items)
            {
                if (tagGroup.Name == site)
                {
                    return tagGroup;
                }
            }
            TagGroup newTagGroup = new TagGroup() {
                Name = site
            };
            Add(newTagGroup);
            return newTagGroup;
        }

        public static void Save(TagGroups tagGroups)
        {
            System.Diagnostics.Debug.WriteLine("Save:Save");
            List<Tag> Tags = new List<Tag>();
            foreach (TagGroup tagGroup in tagGroups)
            {
                foreach (Tag tag in tagGroup.Tags)
                {
                    Tags.Add(tag);
                }
            }
            Models.Tag.Save(Tags);
        }

        public void AddTag(Tag tag)
        {
            TagGroup tagGroup = GroupBySite(tag.Site);
            foreach (Tag _tag in tagGroup.Tags)
            {
                if (_tag.Code == tag.Code)
                {
                    return;
                }
            }
            tagGroup.Tags.Add(tag);
            Save(this);
        }

        public void RemoveTag(Tag tag)
        {
            TagGroup tagGroup = GroupBySite(tag.Site);
            foreach (Tag _tag in tagGroup.Tags)
            {
                if (_tag.Code == tag.Code)
                {
                    _tag.RemovePage();
                    tagGroup.Tags.Remove(_tag);
                    if (tagGroup.Tags.Count == 0)
                    {
                        Items.Remove(tagGroup);
                    }
                    Save(this);
                    return;
                }
            }
        }

        public bool HasTag(Tag tag)
        {
            TagGroup tagGroup = GroupBySite(tag.Site);
            foreach (Tag _tag in tagGroup.Tags)
            {
                if (_tag.Code == tag.Code)
                {
                    return true;
                }
            }
            return false;
        }

        public void ChaneCurrentTag()
        {
            System.Diagnostics.Debug.WriteLine("ChaneCurrentTag");
            foreach (TagGroup tagGroup in Items)
            {
                foreach (Tag tag in tagGroup.Tags)
                {
                    tag.CheckActive();
                }
            }
        }
    }

    class TagGroup
    {
        public string Name { get; set; } = "";
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
    }

    class Tag : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Name { get; set; } = "";
        public string Count { get; set; } = "";
        public string Code { get; set; } = "";
        public string SiteUrl { get; set; } = "";

        private Visibility _IconVisibility { get; set; } = Visibility.Collapsed;
        public Visibility IconVisibility
        {
            get { return _IconVisibility; }
            set
            {
                _IconVisibility = value;
                RaiseProperty("IconVisibility");
            }
        }


        public string Site
        {
            get {
                try
                {
                    return (new Uri(SiteUrl)).Host;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public Tag()
        {
        }

        public void CheckActive()
        {
            if ((App.Settings.current_tag_code == Code) && (SiteUrl == App.Settings.current_site))
            //string[] tags = App.Settings.current_tag_code.Split(new char[] { '+' });
            //if (tags.Contains(Code) && (SiteUrl == App.Settings.current_site))
            {
                IconVisibility = Visibility.Visible;
            } else
            {
                IconVisibility = Visibility.Collapsed;
            }
        }

        public Tag(string arg)
        {
            JsonObject jsonObject = JsonObject.Parse(arg);
            Name = jsonObject.GetNamedString("Name");
            Code = jsonObject.GetNamedString("Code");
            SiteUrl = jsonObject.GetNamedString("SiteUrl");
            CheckActive();
        }

        public new string ToString()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue("Name", JsonValue.CreateStringValue(Name));
            jsonObject.SetNamedValue("Code", JsonValue.CreateStringValue(Code));
            jsonObject.SetNamedValue("SiteUrl", JsonValue.CreateStringValue(SiteUrl));
            return jsonObject.ToString();
        }

        public static List<Tag> Load()
        {
            List<Tag> list = new List<Tag>();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int index = 0;
            while (localSettings.Values.ContainsKey("tags_" + index))
            {
                list.Add(new Tag(localSettings.Values["tags_" + index].ToString()));
                index++;
            }

            return list;
        }

        public void RemovePage()
        {
            Models.Page.Remove(SiteUrl, Code);
        }

        public static void Save(List<Tag> list)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int i = 0;
            for (; i < list.Count(); i++)
            {
                localSettings.Values["tags_" + i] = list[i].ToString();
            }
            while (localSettings.Values.ContainsKey("tags_" + i))
            {
                localSettings.Values.Remove("tags_" + i);
                i++;
            }
        }
    }
}
