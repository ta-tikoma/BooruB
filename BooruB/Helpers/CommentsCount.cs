using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BooruB.Helpers
{
    class CommentsCount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //System.Diagnostics.Debug.WriteLine("value:" + value);
            if (value != null)
            {
                if (value is ObservableCollection<Models.Comment>)
                {
                    return "COMMENTS (" + (value as ObservableCollection<Models.Comment>).Count() + ")";
                }
            }
            return "COMMENTS (0)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
