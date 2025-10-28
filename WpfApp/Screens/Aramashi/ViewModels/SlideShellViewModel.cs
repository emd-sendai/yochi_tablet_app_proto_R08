using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;


namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class SlideShellViewModel : ObservableObject
    {
        private int _index = 0;
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public ObservableCollection<SlideContent> Slides { get; } = new();

        [ObservableProperty]
        private SlideContent? currentSlide;

        public bool CanGoPrev => _index > 0;
        public bool CanGoNext => _index < Slides.Count - 1;

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 1.0;
        public double MaxScale { get; } = 1.3;

        public SlideShellViewModel(string screenName, NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            LoadSlidesFromJson(screenName);
            CurrentSlide = Slides.Count > 0 ? Slides[0] : null;
        }

        private void LoadSlidesFromJson(string screenName)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var jsonPath = Path.Combine(baseDir, "SlideResources", screenName, "slides.json");

            if (!File.Exists(jsonPath))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] slides.json not found: {jsonPath}");
                return;
            }

            var jsonText = File.ReadAllText(jsonPath);
            var slideList = JsonSerializer.Deserialize<List<SlideContent>>(jsonText);
            if (slideList == null) return;

            Slides.Clear();
            string slideDir = Path.GetDirectoryName(jsonPath)!;

            foreach (var slide in slideList)
            {
                // 画像パスを補完
                slide.ResolvePaths(slideDir);

                // SlideContent は Title / Description / ImagePath をそのまま保持
                Slides.Add(slide);
            }
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
        public void ZoomIn()
        {
            Zoom(1.1); // 1.2倍拡大
        }

        [RelayCommand]
        public void ZoomOut()
        {
            Zoom(1 / 1.1); // 1.2分の1に縮小
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        public void GoToSlide(int index)
        {
            if (index >= 0 && index < Slides.Count)
            {
                _index = index;
                CurrentSlide = Slides[_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        [RelayCommand]
        private void Next()
        {
            if (_index < Slides.Count - 1)
            {
                CurrentSlide = Slides[++_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        [RelayCommand]
        private void Prev()
        {
            if (_index > 0)
            {
                CurrentSlide = Slides[--_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

    }
}
