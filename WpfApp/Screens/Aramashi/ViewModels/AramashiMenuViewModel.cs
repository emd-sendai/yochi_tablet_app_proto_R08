using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Common.ViewModels.VideoPlayerViewModel;
using static WpfApp.Screens.Aramashi.ViewModels.PresentationViewerViewModel;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    /// <summary>
    /// AramashiMenuView.xamlに対応するViewModel。
    /// 補償のあらまし画面での処理を行う。
    /// </summary>
    public partial class AramashiMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        [ObservableProperty]
        private ObservableCollection<ButtonConfig> aramashiButtons;

        [ObservableProperty]
        private ObservableCollection<ButtonConfig> othersButtons;

        /// <summary>
        /// 用地補償の手順の番号をキー、各手順の最初のページを値として保持する。
        /// </summary>
        private Dictionary<int, int> SectionPageCounts = new();

        public AramashiMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _message = new MessageDialogService();
            _path = new AppPathService();
            
            AramashiButtons = LoadButtonFromJson(_path.AramashiButtonPath);
            OthersButtons = LoadButtonFromJson(_path.OthersButtonPath);

            LoadSlidesFromJsons();
        }

        /// <summary>
        /// ボタンに表示する名称を取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ObservableCollection<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new ObservableCollection<ButtonConfig>();

            if (!File.Exists(path))
            {
                _message.ShowMessage("ボタンの名称を表示できません。");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list == null || list.Count <= 0)
                {
                    _message.ShowMessage("ボタンの名称を表示できません。");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                }
                else
                {
                    list = list.OrderBy(x => x.BtnId).ToList();

                    foreach (var btn in list)
                    {
                        buttons.Add(btn);
                    }
                    Debug.WriteLine($"[DEBUG] ボタンデータ読み込み完了: {path} 件数 = {buttons.Count}");
                }
            }

            return buttons;
        }

        /// <summary>
        /// sectionIndexと一致する用地補償の手順の最初のページ番号を取得する。
        /// 一致する手順番号が存在しない場合は1ページを表示する。
        /// </summary>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        private int GetSectionFirstPages(int sectionIndex)
        {
            int firstPage = 0;

            if(SectionPageCounts.TryGetValue(sectionIndex, out int pageCount))
            {
                firstPage = pageCount;
            }

            return firstPage;
        }

        /// <summary>
        /// 補償のあらましの各手順の表示内容の設定ファイルのパスを取得する。
        /// </summary>
        /// <returns></returns>
        private List<string> GetSlidePaths()
        {
            List<string> paths = new();
            var dataFolder = _path.AramashiSlideDire;

            for (int i = 1; ; i++)
            {
                var aramashiPath = Path.Combine(dataFolder, $"slides_aramashi{i}.json");
                if (!Path.Exists(aramashiPath)) break;

                paths.Add(aramashiPath);
                Debug.WriteLine($"[DEBUG] スライドパスを追加: {aramashiPath}");
            }

            return paths;
        }

        /// <summary>
        /// 補償のあらましの各手順の設定ファイルのパスから表示内容を取得する。
        /// </summary>
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

        /// <summary>
        /// メインメニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// 動画再生画面で補償のあらましの動画（統合版）を表示する。
        /// </summary>
        [RelayCommand]
        private void OpenVideoPlayer()
        {
            // 非モーダル表示
            _dia.ShowWindow<VideoPlayerViewModel> (_path.MediaMeinAramashiDire, MediaCategory.Main);
        }

        /// <summary>
        /// PDF資料選択画面で補償のあらましのPDF資料を表示する。
        /// </summary>
        [RelayCommand]
        private void OpenPdfDocuments()
        {
            var folderPath = _path.AramashiPdfDire;
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("PDFファイルが存在しません。");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }

        /// <summary>
        /// 用地補償の手順 1 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection1()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(1));
        }

        /// <summary>
        /// 用地補償の手順 2 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection2()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(2));
        }

        /// <summary>
        /// 用地補償の手順 3 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection3()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(3));
        }

        /// <summary>
        /// 用地補償の手順 4 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection4()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(4));
        }

        /// <summary>
        /// 用地補償の手順 5 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection5()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(5));
        }

        /// <summary>
        /// 用地補償の手順 6 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection6()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(6));
        }

        /// <summary>
        /// 用地補償の手順 7 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection7()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(7));
        }

        /// <summary>
        /// 用地補償の手順 8 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection8()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(8));
        }

        /// <summary>
        /// 用地補償の手順 9 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection9()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(9));
        }

        /// <summary>
        /// 用地補償の手順 10 の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSection10()
        {
            var vm = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Aramashi);
            _nav.Navigate(vm);
            vm.GoToSlide(GetSectionFirstPages(10));
        }

        /// <summary>
        /// その他 1 （代替地）の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToDaitaichi()
        {
            _nav.Navigate(new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Others001));
        }

        /// <summary>
        /// その他 2 （税金等）の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToZeikin()
        {
            _nav.Navigate(new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Others002));
        }

        /// <summary>
        /// その他 3 （損失補償基準）の最初のページを表示する。
        /// </summary>
        [RelayCommand]
        private void GoToSonshitsu()
        {
            _nav.Navigate(new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Others003));
        }
    }
}
