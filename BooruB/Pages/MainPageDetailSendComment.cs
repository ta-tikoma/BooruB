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
        TextBox CommentInput = null;
        Button CommentSend = null;

        private void CommentInput_Loaded(object sender, RoutedEventArgs e)
        {
            CommentInput = sender as TextBox;
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            CommentSend = sender as Button;
        }

        private async void CommentSend_Click(object sender, RoutedEventArgs e)
        {
            CommentSend.IsEnabled = false;
            CommentInput.IsEnabled = false;
            (CommentSend.Resources["SyncShow"] as Storyboard).Begin();
            ImageData.LoadComments(await App.Settings.Query.Comment(ImageData.Id, CommentInput.Text));
            (CommentSend.Resources["SyncShow"] as Storyboard).Stop();
            CommentSend.IsEnabled = true;
            CommentInput.IsEnabled = true;
            CommentInput.Text = "";
            //System.Diagnostics.Debug.WriteLine("CommentSend:");
        }
    }
}
