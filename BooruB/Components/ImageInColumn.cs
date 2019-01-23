using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace BooruB.Components
{
    class ItemData
    {
        public Models.Image ImageData = null;
        public Image ImageComponent = null;
        public double MarginTop = 0;
        public double Height = 0;
        public int Column = 0;

        public bool isRender = false;


        public const string USE = "use";
        public const string UNUSE = "unuse";

        public bool IsVisible(double VerticalOffset)
        {
            if (
                // нижний край ниже начала экрана
                (MarginTop + Height >= VerticalOffset)
                &&
                // верхний край выше конца экрана
                (MarginTop <= VerticalOffset + ((Frame)Window.Current.Content).ActualHeight)
                )
            {
                return true;
            }
            return false;
        }

        private async Task<BitmapImage> GetBitmapImage(string url)
        {
            StorageFile tempFile = null;

            string type = Models.Image.GetType(url);
            string hash = Models.Page.GetHash2(url);

            try
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(hash + type);
            }
            catch (Exception)
            {
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(hash + type, CreationCollisionOption.OpenIfExists);
                await App.Settings.Query.DownloadFile(url, tempFile);
            }

            if (tempFile == null)
            {
                return null;
            }

            BitmapImage _bitmapImage = new BitmapImage()
            {
                DecodePixelType = DecodePixelType.Physical
            };
            try
            {
                using (IRandomAccessStream fileStream = await tempFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    await _bitmapImage.SetSourceAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + ex.Message);
            }

            tempFile = null;

            return _bitmapImage;
        }

        public ItemData()
        {
            //storyboard = new Storyboard();
            //fadeInThemeAnimation = new FadeInThemeAnimation();
            //storyboard.Children.Add(fadeInThemeAnimation);
        }

        public async Task Show(KeyValuePair<Image, Storyboard> imageStoryboard)
        {
            if (isRender)
            {
                return;
            }
            isRender = true;
            this.ImageComponent = imageStoryboard.Key;

            Models.Page.Save(ImageData.Page, ImageData.NextPageLink);

            BitmapImage _bitmapImage = await GetBitmapImage(ImageData.ThumbnailUrl);
            //System.Diagnostics.Debug.WriteLine("ImageData.ThumbnailUrl:" + ImageData.ThumbnailUrl);
            //System.Diagnostics.Debug.WriteLine("PixelWidth:" + _bitmapImage.PixelWidth);
            //System.Diagnostics.Debug.WriteLine("PixelHeight:" + _bitmapImage.PixelHeight);
            double _height = App.Settings.side_size;
            if (_bitmapImage.PixelWidth != 0)
            {
                _height = ((double)App.Settings.side_size / (double)_bitmapImage.PixelWidth) * (double)_bitmapImage.PixelHeight;
            }
            //System.Diagnostics.Debug.WriteLine("Height:" + Height);
            Height = _height + 4;

            ImageComponent.Height = _height;
            ImageComponent.Margin = new Thickness(0, MarginTop, 0, 0);
            ImageComponent.DataContext = ImageData;
            ImageComponent.Source = _bitmapImage;
            imageStoryboard.Value.Stop();
            imageStoryboard.Value.Begin();
            //ImageComponent.Opacity = 1;
            Grid.SetColumn(ImageComponent, Column);
        }

        public void Hide()
        {
            if (!isRender)
            {
                return;
            }
            isRender = false;
            //System.Diagnostics.Debug.WriteLine("Hide");
            ImageComponent.Tag = UNUSE;
            ImageComponent.Opacity = 0;
            ImageComponent = null;
        }
    }

    class ImageInColumn : Grid
    {
        List<ItemData> ItemsData = new List<ItemData>();
        Dictionary<Image, Storyboard> dict = new Dictionary<Image, Storyboard>();

        public double VerticalOffset = 0;

        // конструктор
        public ImageInColumn()
        {
            for (int i = 0; i < App.Settings.images_in_row; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }
            //System.Diagnostics.Debug.WriteLine("ImageInColumn:");
        }

        // проверка "по высоте"
        private bool CheckHeights()
        {
            // подготовим высоты
            List<double> heights = new List<double>();
            for (int i = 0; i < App.Settings.images_in_row; i++)
            {
                heights.Add(0);
            }

            foreach (ItemData data in ItemsData)
            {
                heights[data.Column] += data.Height;
            }

            double maxHeight = 0;
            foreach (double height in heights)
            {
                if (maxHeight < height)
                {
                    maxHeight = height;
                }
            }

            MinHeight = maxHeight;

            // проверим высоты
            foreach (double height in heights)
            {
                if (height - VerticalOffset - ((Frame)Window.Current.Content).ActualHeight < 0)
                {
                    return true;
                }
            }

            return false;
        }

        // возвращает иденкс минимальной колонки и её высоту
        private KeyValuePair<int, double> GetMinColumn()
        {
            // подготовим высоты
            List<double> heights = new List<double>();
            for (int i = 0; i < App.Settings.images_in_row; i++)
            {
                heights.Add(0);
            }

            foreach (ItemData data in ItemsData)
            {
                //System.Diagnostics.Debug.WriteLine("data.Height:" + data.Height);
                heights[data.Column] += data.Height;
            }


            int colimn = 0;
            double height = heights[colimn];

            for (int i = 0; i < heights.Count; i++)
            {
                if (heights[i] < height)
                {
                    height = heights[i];
                    colimn = i;
                }
            }

            //System.Diagnostics.Debug.WriteLine("colimn:" + colimn);
            //System.Diagnostics.Debug.WriteLine("height:" + height);


            return new KeyValuePair<int, double>(colimn, height);
        }

        // проверка "на экране"
        public void CheckVisible(double VerticalOffset)
        {
            this.VerticalOffset = VerticalOffset;

            //System.Diagnostics.Debug.WriteLine("CheckVisible");
            foreach (ItemData data in ItemsData)
            {
                if (data.IsVisible(VerticalOffset))
                {
                    if (!data.isRender)
                    {
                        data.Show(GetImage());
                    }
                } else
                {
                    if (data.isRender)
                    {
                        data.Hide();
                    }
                }
            }

            if (CheckHeights())
            {
                ImagesLoad();
            }
        }

        // отдает изображение новое или неиспользуемое
        public event EventHandler<Models.Image> ImageTapped;
        private KeyValuePair<Image, Storyboard> GetImage()
        {
            foreach (Image image in Children)
            {
                if (image.Tag.ToString() == ItemData.UNUSE)
                {
                    //System.Diagnostics.Debug.WriteLine("ItemData.UNUSE");
                    image.Tag = ItemData.USE;
                    return new KeyValuePair<Image, Storyboard>(image, dict[image]);
                }
            }
            Image newImage = new Image()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = App.Settings.side_size,
                Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                Tag = ItemData.USE,
                Opacity = 0
            };
            newImage.Tapped += NewImage_Tapped;
            Children.Add(newImage);

            Storyboard storyboard = new Storyboard();
            DoubleAnimation fadeInThemeAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };

            storyboard.Children.Add(fadeInThemeAnimation);
            Storyboard.SetTarget(fadeInThemeAnimation, newImage);
            Storyboard.SetTargetProperty(fadeInThemeAnimation, "Opacity");
            dict.Add(newImage, storyboard);

            return new KeyValuePair<Image, Storyboard>(newImage, storyboard);
        }

        private void NewImage_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ImageTapped != null)
            {
               ImageTapped(sender, (sender as Image).DataContext as Models.Image);
            }
        }

        // сброс
        public void Reset(Models.Page page = null)
        {
            //System.Diagnostics.Debug.WriteLine("Reset");
            VerticalOffset = 0;
            MinHeight = 0;
            ItemsData.Clear();
            Children.Clear();
            Models.Images Images = DataContext as Models.Images;
            Images.ClearSelf(page);
            busy = false;
            ImagesLoad();
        }

        // загрузка
        bool busy = false;
        System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
        public async Task ImagesLoad()
        {
            if (busy)
            {
                return;
            }
            busy = true;
            //System.Diagnostics.Debug.WriteLine("ImageLoad:begin");

            Pages.MainPage.ShowListLoading();
            Models.Images Images = DataContext as Models.Images;

            while (CheckHeights())
            {
                //System.Diagnostics.Debug.WriteLine("while");
                uint c = await Images.Load();
                if (c == 0)
                {
                    Pages.MainPage.HideListLoading();
                    return;
                }
                System.Diagnostics.Debug.WriteLine("c.Count:" + c);
                for (int i = Images.Count() - (int) c; i < Images.Count(); i++)
                {
                    KeyValuePair<int, double> columnHeight = GetMinColumn();

                    ItemData itemData = new ItemData()
                    {
                        Column = columnHeight.Key,
                        MarginTop = columnHeight.Value,
                        ImageData = Images[i]
                    };
                    await itemData.Show(GetImage());
                    ItemsData.Add(itemData);
                }
            }
            Pages.MainPage.HideListLoading();
            //System.Diagnostics.Debug.WriteLine("ImageLoad:end");
            busy = false;
        }
    }
}
