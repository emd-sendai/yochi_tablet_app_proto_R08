using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// VideoPlayerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        public VideoPlayerWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                if (DataContext is VideoPlayerViewModel vm)
                {
                    vm.PlayRequested += () => Media.Play();
                    vm.PauseRequested += () => Media.Pause();
                    vm.StopRequested += () => Media.Stop();
                    vm.PositionProvider = () => Media.Position;
                    vm.SeekAction = time => Media.Position = time;

                    Media.MediaOpened += (_, _) =>
                    {
                        vm.Duration = Media.NaturalDuration.TimeSpan;
                    };
                }
            };

            Closing += VideoPlayerWindow_Closing;

        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (DataContext is VideoPlayerViewModel vm)
            {
                vm.Duration = Media.NaturalDuration.TimeSpan;
                Media.Play();
            }
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
            => (DataContext as VideoPlayerViewModel)?.BeginSeek();

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
            => (DataContext as VideoPlayerViewModel)?.EndSeek();

        private void VideoPlayerWindow_Closing(object? sender, CancelEventArgs e)
        {
            // 再生停止＆リソース解放
            Media.Stop();
            Media.Source = null;
        }
    }
}
