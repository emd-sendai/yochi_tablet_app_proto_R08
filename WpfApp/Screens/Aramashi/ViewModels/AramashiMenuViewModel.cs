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
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class AramashiMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        private static readonly string ButtonDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ButtonSettings");

        public ObservableCollection<ButtonConfig> Buttons { get; } = new();

        private Dictionary<int, int> SectionPageCounts = new();

        public AramashiMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            LoadButtonFromJson();
            LoadSlidesFromJsons();
        }

        private void LoadButtonFromJson()
        {
            try
            {
                string path = Path.Combine(ButtonDataFolder, "aramashiButtons.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("ボタンデータが見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list == null)
                {
                    MessageBox.Show("ボタンデータが空です");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                    return;
                }

                Debug.WriteLine($"[DEBUG] 読み込み完了: {path} 件数 = {list.Count}");
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

        private int GetSectionFirstPages(int sectionIndex)
        {
            int firstPage = 0;

            if(SectionPageCounts.TryGetValue(sectionIndex, out int pageCount))
            {
                firstPage = pageCount;
            }

            return firstPage;
        }

        private List<string> GetSlidePaths()
        {
            List<string> paths = new();
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SlideResources", "Aramashi");

            for (int i = 1; ; i++)
            {
                var aramashiPath = Path.Combine(dataFolder, $"slides_aramashi{i}.json");
                if (!Path.Exists(aramashiPath)) break;

                paths.Add(aramashiPath);
                Debug.WriteLine($"[DEBUG] スライドパスを追加: {aramashiPath}");
            }

            return paths;
        }

        private void LoadSlidesFromJsons()
        {
            var paths = GetSlidePaths();

            int pageCount = 0;
            int pageIndex = 1;

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var jsonText = File.ReadAllText(path);
                    var slides = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (slides != null)
                    {
                        SectionPageCounts.Add(pageIndex, pageCount);
                        pageCount += slides.Count;
                    }
                    else
                    {
                        Debug.WriteLine($"[DEBUG] スライドデータが空です: {path}");
                        break;
                    }
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] スライドデータが見つかりません: {path}");
                    break;
                }
                pageIndex++;
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
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(1));
        }

        [RelayCommand]
        private void GoToSection2()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(2));
        }

        [RelayCommand]
        private void GoToSection3()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(3));
        }

        [RelayCommand]
        private void GoToSection4()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(4));
        }

        [RelayCommand]
        private void GoToSection5()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(5));
        }

        [RelayCommand]
        private void GoToSection6()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(6));
        }

        [RelayCommand]
        private void GoToSection7()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(7));
        }

        [RelayCommand]
        private void GoToSection8()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(8));
        }

        [RelayCommand]
        private void GoToSection9()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(9));
        }

        [RelayCommand]
        private void GoToSection10()
        {
            var vm = new PresentationViewerViewModel("aramashi", _nav, _dia);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(10));
        }

        [RelayCommand]
        private void GoToDaitaichi()
        {
            _nav.Navigate(new PresentationViewerViewModel("daitaichi", _nav, _dia));
        }

        [RelayCommand]
        private void GoToZeikin()
        {
            _nav.Navigate(new PresentationViewerViewModel("zeikin", _nav, _dia));
        }

    }
}
