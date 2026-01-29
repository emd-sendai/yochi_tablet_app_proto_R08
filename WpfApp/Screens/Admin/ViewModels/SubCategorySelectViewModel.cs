using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// SubCategorySelectView.xamlに対応するViewModel。
    /// 前の画面で選択された表示設定項目からサブ項目を選択して、該当する画面に遷移する。
    /// </summary>
    public partial class SubCategorySelectViewModel : ObservableObject, INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;

        public Func<object>? CreateBackViewModel => () => new DisplaySettingsViewModel(_nav, _dia);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private readonly DisplaySettingsCategory _mainCategory;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private ObservableCollection<string> buttonLabels;

        private static readonly string _titleItem = "詳細項目選択";

        public Dictionary<DisplaySettingsCategory, string> ScreenNames { get; } = new Dictionary<DisplaySettingsCategory, string>
        {
            { DisplaySettingsCategory.VideoSettings, "動画設定" }
        };

        public enum VideoSettingsSubCategory
        {
            VideoFileSettings,      // 動画ファイル設定
            VideoChapterSettings,   // 統合版動画チャプター設定
        }

        public Dictionary<VideoSettingsSubCategory, string> SettingsSubCategoryNames { get; } = new Dictionary<VideoSettingsSubCategory, string>
        {
            { VideoSettingsSubCategory.VideoFileSettings, "動画ファイル" },
            { VideoSettingsSubCategory.VideoChapterSettings, "統合版動画チャプター" }
        };

        public SubCategorySelectViewModel(NavigationService navigationService, IDialogService dialogService, DisplaySettingsCategory mainCategory)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();

            _mainCategory = mainCategory;
            Title = GetTitle();
            ButtonLabels = GetButtonLabels();

            // 格納されたボタンの名称が２つ未満の場合
            if (ButtonLabels.Count < 2)
            {
                // 仮の名称を格納する
                ButtonLabels = new ObservableCollection<string>() { "", "" };
                // メッセージを表示する
                Debug.WriteLine($"[ERROR] _mainCategoryの値が不正です: DisplaySettingsCategory.{_mainCategory}");
                _message.ShowMessage("選択可能な項目が存在しません。\n前の画面に戻って、もう一度お試しください。");
            }
        }

        /// <summary>
        /// 親項目のタイトルを取得する。
        /// </summary>
        /// <returns></returns>
        private string? GetMainCategoryTitle()
        {
            ScreenNames.TryGetValue(_mainCategory, out var mainCategoryTitle);

            return mainCategoryTitle;
        }

        /// <summary>
        /// 画面タイトルを取得する。
        /// </summary>
        /// <returns></returns>
        private string GetTitle()
        {
            // タイトルに初期値を設定
            var title = _titleItem;

            var mainCategoryTitle = GetMainCategoryTitle();

            if (!string.IsNullOrWhiteSpace(mainCategoryTitle))
            {
                title = mainCategoryTitle + " - " + _titleItem;
            }

            return title;
        }

        /// <summary>
        /// ボタンに表示する詳細項目の名称を取得する。
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<string> GetButtonLabels()
        {
            var labels = new ObservableCollection<string>();

            if(_mainCategory == DisplaySettingsCategory.VideoSettings)
            {
                // １番目の項目ボタン
                if (SettingsSubCategoryNames.TryGetValue(VideoSettingsSubCategory.VideoFileSettings, out var name1)) { labels.Add(name1); }
                // ２番目の項目ボタン
                if (SettingsSubCategoryNames.TryGetValue(VideoSettingsSubCategory.VideoChapterSettings, out var name2)) { labels.Add(name2); }
            }

            return labels;
        }

        [RelayCommand]
        private void ToggleDrawer()
        {
            IsLeftDrawerOpen = !IsLeftDrawerOpen;
        }

        [RelayCommand]
        private void GoToDisplaySetttings()
        {
            _nav.Navigate(new DisplaySettingsViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAdminDocument()
        {
            _nav.Navigate(new AdminDocumentViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoBack()
        {
            _nav.GoBack();
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));

        }

        /// <summary>
        /// 画面の上から１番目のボタンに設定されたサブ項目に対応する画面へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToSubCategory1()
        {
            if (_mainCategory == DisplaySettingsCategory.VideoSettings)
            {
                _nav.Navigate(new VideoFileSettingsViewModel(_nav, _dia));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _mainCategoryの値が不正です: DisplaySettingsCategory.{_mainCategory}");
                _message.ShowMessage("次の画面を表示できません。\n前の画面に戻って、もう一度お試しください。");
            }
        }

        /// <summary>
        /// 画面の上から２番目のボタンに設定されたサブ項目に対応する画面へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToSubCategory2()
        {
            if (_mainCategory == DisplaySettingsCategory.VideoSettings)
            {
                _nav.Navigate(new VideoChapterSettingsViewModel(_nav, _dia));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _mainCategoryの値が不正です: DisplaySettingsCategory.{_mainCategory}");
                _message.ShowMessage("次の画面を表示できません。\n前の画面に戻って、もう一度お試しください。");
            }
        }
    }
}
