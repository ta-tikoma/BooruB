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
using Windows.UI.Xaml.Media.Animation;

namespace BooruB.Pages
{
    public sealed partial class MainPage : Page
    {
        // список загружается
        private static int counter = 0;

        private static Storyboard ListLoadingRotation = null;
        private static Storyboard ListLoadingShow = null;
        private static Storyboard ListLoadingHide = null;
        
        public static void ShowListLoading()
        {
            if (counter == 0)
            {
                ListLoadingRotation?.Begin();
                ListLoadingShow?.Begin();
            }

            counter++;
        }

        public static void HideListLoading()
        {
            counter--;
            if (counter == 0)
            {
                ListLoadingHide?.Begin();
                ListLoadingRotation?.Stop();
            }
        }


        // ничего не найдено
        private static Storyboard NothingFoundShow = null;
        private static Storyboard NothingFoundHide = null;
        private static TextBlock NothingFoundText = null;

        public static bool NF = false;

        public static bool IsNothingFound()
        {
            bool _nf = NF;
            NF = !NF;
            return _nf;
        }

        public static void ShowNothingFound()
        {
            NF = true;
            NothingFoundShow?.Begin();
        }

        public static void HideNothingFound()
        {
            NF = false;
            NothingFoundHide?.Begin();
        }

        public static void SetNothingFoundText(string text)
        {
            NothingFoundText.Text = text;
        }
    }
}
