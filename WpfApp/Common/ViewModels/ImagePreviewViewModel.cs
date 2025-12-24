using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp.Common.ViewModels
{
    public partial class ImagePreviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        [ObservableProperty]
        private BitmapImage? image;

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 0.1;
        public double MaxScale { get; } = 5.0;


        public ImagePreviewViewModel(string imagePath) 
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show("画像の読み込みができません");
                Debug.WriteLine($"[DEBUG] imagePathが空です");
                Close();
            }
            else if(!File.Exists(imagePath))
            {
                MessageBox.Show("画像の読み込みができません");
                Debug.WriteLine($"[DEBUG] 画像が存在しません: {imagePath}");
                Close();
            }
            else
            {
                LoadImage(imagePath);
            }
        }

        public void LoadImage(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック防止
            bitmap.UriSource = new Uri(imagePath);
            bitmap.EndInit();

            Image = bitmap;
        }

        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
