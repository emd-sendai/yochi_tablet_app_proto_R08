using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    public partial class VideoPlayerViewModel : ObservableObject
    {
        private readonly string _videoPath;

        public string VideoPath => _videoPath;

        public VideoPlayerViewModel(string videoFileName)
        {
            _videoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaResources", videoFileName);
        }

        [RelayCommand]
        private void Play(MediaElement? media)
        {
            if (media == null) return;
            Application.Current.Dispatcher.Invoke(() => media.Play());
        }

        [RelayCommand]
        public void Pause(MediaElement media) => media.Pause();

        [RelayCommand]
        public void Stop(MediaElement media) => media.Stop();

        [RelayCommand]
        public void Close(Window window) => window.Close();
    }
}
