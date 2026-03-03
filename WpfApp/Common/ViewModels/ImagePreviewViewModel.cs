using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// ImagePreviewWindow.xamlに対応するViewModel。
    /// 画像プレビュー画面での処理を行う。
    /// </summary>
    public partial class ImagePreviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        /// <summary>
        /// 画像データ
        /// </summary>
        [ObservableProperty]
        private BitmapImage? image;

        /// <summary>
        /// 表示倍率
        /// </summary>
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

        /// <summary>
        /// imagePathから画像を読み込み、BitmapImageとしてUI要素に設定する。
        /// </summary>
        /// <param name="imagePath"></param>
        public void LoadImage(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック防止
            bitmap.UriSource = new Uri(imagePath);
            bitmap.EndInit();

            Image = bitmap;
        }

        /// <summary>
        /// 画像の表示倍率を変更する。
        /// </summary>
        /// <param name="factor"></param>
        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        /// <summary>
        /// 画像を拡大する。
        /// </summary>
        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        /// <summary>
        /// 画像を縮小する。
        /// </summary>
        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        /// <summary>
        /// 画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
