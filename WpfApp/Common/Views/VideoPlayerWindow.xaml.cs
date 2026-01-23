using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// VideoPlayerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        private DispatcherTimer? _timer;
        private bool _isDragging;

        public VideoPlayerWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                if (DataContext is VideoPlayerViewModel vm)
                {
                    Media.MediaOpened += Media_MediaOpened;

                    vm.PlayRequested += () =>
                    {
                        Media.Play();
                        vm.IsPlaying = true;
                        _timer?.Start();
                    };
                    
                    vm.PauseRequested += () =>
                    {
                        Media.Pause();
                        vm.IsPlaying = false;
                        _timer?.Stop();
                    };

                    vm.SeekRequested += time =>
                    {
                        if (Media.NaturalDuration.HasTimeSpan)
                        {
                            Media.Position = time;

                            // 直接現在の再生位置を設定（UI要素の更新）
                            vm.CurrentPosition = time;
                        }
                    };

                    _timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(200)
                    };

                    _timer.Tick += (_, _) =>
                    {
                        if (!_isDragging && Media.NaturalDuration.HasTimeSpan)
                        {
                            vm.CurrentPosition = Media.Position;
                        }
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
                vm.CurrentPosition = TimeSpan.Zero;
            }
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;

            if (DataContext is VideoPlayerViewModel vm)
            {
                Media.Position = vm.CurrentPosition;
            }
        }

        private void VideoPlayerWindow_Closing(object? sender, CancelEventArgs e)
        {
            // 再生停止＆リソース解放
            Media.Stop();
            Media.Source = null;
        }
    }
}
