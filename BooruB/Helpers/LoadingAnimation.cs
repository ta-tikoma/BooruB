using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace BooruB.Helpers
{
    public class LoadingAnimation
    {
        public static Storyboard RotationLoadingIcon = null;
        public static Storyboard ShowLoadingButton = null; 
        public static Storyboard HideLoadingButton = null;
        public static Storyboard ShowNothingFound = null;
        public static Storyboard HideNothingFound = null;

        public static int Counter = 0;
        public static bool NF = false;

        public static bool NothingFound()
        {
            bool _nf = NF;
            NF = !NF;
            return _nf;
        }

        public static void ShowNF()
        {
            NF = true;
            ShowNothingFound?.Begin();
        }

        public static void HideNF()
        {
            NF = false;
            HideNothingFound?.Begin();
        }

        public static void Show()
        {
            if (Counter == 0)
            {
                RotationLoadingIcon?.Begin();
                ShowLoadingButton?.Begin();
            }

            Counter++;
        }

        public static void Hide()
        {
            Counter--;
            if (Counter == 0)
            {
                HideLoadingButton?.Begin();
                RotationLoadingIcon?.Stop();
            }
        }
    }
}
