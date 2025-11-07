using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class AramashiMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public ObservableCollection<ButtonData> Buttons { get; } = new();

        public AramashiMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            LoadButtonFromJson();
        }

        private void LoadButtonFromJson()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/aramashiButtons.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("ボタンデータが見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonData>>(json) ?? new List<ButtonData>();

                Buttons.Clear();
                foreach (var btn in list)
                    Buttons.Add(btn);

            }
            catch (Exception ex)
            {
                MessageBox.Show("ボタンデータを読み込むことができません");
                Debug.WriteLine($"[DEBUG] ボタンデータを読み込むことができません: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void OpenVideoPlayer()
        {
            // 非モーダル表示
            _dia.ShowWindow<VideoPlayerViewModel> ("AramashiSample.mp4");
        }

        [RelayCommand]
        private void GoToSection1()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(0);
        }

        [RelayCommand]
        private void GoToSection2()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(1);
        }

        [RelayCommand]
        private void GoToSection3()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(2);
        }

    }
}
