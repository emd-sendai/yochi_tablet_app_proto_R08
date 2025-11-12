using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class PresentationViewerViewModel : ObservableObject
    {
        private int _index = 0;
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public ObservableCollection<PresentationPageConfig> Slides { get; } = new();

        [ObservableProperty]
        private PresentationPageConfig? currentSlide;

        public bool CanGoPrev => _index > 0;
        public bool CanGoNext => _index < Slides.Count - 1;

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 1.0;
        public double MaxScale { get; } = 2.0;

        private readonly string _jsonPath;

        public PresentationViewerViewModel(string presentationName, NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _jsonPath = Path.Combine(baseDir, "SlideResources", presentationName, "slides.json");

            LoadSlidesFromJson();
            CurrentSlide = Slides.Count > 0 ? Slides[0] : null;
        }

        private void LoadSlidesFromJson()
        {
            if (!File.Exists(_jsonPath))
            {
                MessageBox.Show("スライドデータが見つかりません。");
                return;
            }

            var jsonText = File.ReadAllText(_jsonPath);
            var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (slideList == null) return;

            Slides.Clear();
            string slideDir = Path.GetDirectoryName(_jsonPath)!;

            foreach (var slide in slideList)
            {
                slide.ResolvePaths(slideDir);
                Slides.Add(slide);
            }
        }

        private void SaveSlidesToJson()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Slides, options);
                File.WriteAllText(_jsonPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存エラー: {ex.Message}");
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
        public void ZoomIn() => Zoom(1.1);

        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        [RelayCommand]
        private void Next()
        {
            SaveSlidesToJson();
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
            SaveSlidesToJson();
            if (_index > 0)
            {
                CurrentSlide = Slides[--_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
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

        public void UpdateCheckBoxState(string slideId, string checkBoxId, bool isChecked)
        {
            var slide = Slides.FirstOrDefault(s => s.Id == slideId);
            if (slide == null) return;

            var section = slide.Sections.FirstOrDefault(sec => sec.CheckBox?.Id == checkBoxId);
            if (section?.CheckBox != null)
            {
                section.CheckBox.IsChecked = isChecked;
                SaveSlidesToJson();
            }
        }
    }
}
