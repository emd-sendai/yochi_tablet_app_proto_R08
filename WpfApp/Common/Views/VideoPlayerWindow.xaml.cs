using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp.Common.ViewModels;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// VideoPlayerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        private readonly IMessageDialogService _message = new MessageDialogService();

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

                    // 動画を再生する
                    vm.PlayRequested += () =>
                    {
                        try
                        {
                            Media.Play();
                            vm.IsPlaying = true;
                            _timer?.Start();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 動画の再生に失敗 : {ex.Message}");
                            _message.ShowMessage("動画の再生に失敗しました。");
                            throw;
                        }

                    };
                    
                    // 動画を一時停止する
                    vm.PauseRequested += () =>
                    {
                        try
                        {
                            Media.Pause();
                            vm.IsPlaying = false;
                            _timer?.Stop();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 動画の一時停止に失敗 : {ex.Message}");
                            _message.ShowMessage("動画の一時停止に失敗しました。");
                            throw;
                        }
                    };

                    // 動画の再生時間を変更する
                    vm.SeekRequested += time =>
                    {
                        try
                        {
                            if (Media.NaturalDuration.HasTimeSpan)
                            {
                                Media.Position = time;

                                // 直接現在の再生位置を設定（UI要素の更新）
                                vm.CurrentPosition = time;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 動画の再生時間の変更に失敗 : {ex.Message}");
                            _message.ShowMessage("動画の再生時間の変更に失敗しました。");
                            throw;
                        }
                    };

                    _timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(200)
                    };

                    _timer.Tick += (_, _) =>
                    {
                        try
                        {
                            if (!_isDragging && Media.NaturalDuration.HasTimeSpan)
                            {
                                vm.CurrentPosition = Media.Position;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 動画の再生時間の変更に失敗 : {ex.Message}");
                            throw;
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
