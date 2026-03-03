using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// VideoChapterSettingsView.xamlに対応するViewModel。（作成中）
    /// 動画チャプター設定画面での処理を行う。
    /// </summary>
    public partial class VideoChapterSettingsViewModel : ObservableObject, INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly ILoggingService _log;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IVideoService _video;

        public Func<object>? CreateBackViewModel => () => new SubCategorySelectViewModel(_nav, _dia, DisplaySettingsCategory.VideoSettings);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        /// <summary>
        /// 一覧表示用チャプターデータリスト
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ChapterItemData> chapterItems;

        /// <summary>
        /// 選択されたチャプターデータ
        /// </summary>
        [ObservableProperty]
        private ChapterItemData? selectedChapterItem;

        public VideoChapterSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _log = new LoggingService();
            _message = new MessageDialogService();
            _path = new AppPathService();
            _video = new VideoService();

            ChapterItems = _video.GetVideoChapterDatas(_path.MediaMeinAramashiChapterPath);
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
        /// チャプターデータを新規追加する。
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task Add()
        {
            var chapterItemData = new ChapterItemData(new ChapterData());

            var editedChapterItem = await _dia.ShowDialogAsync<VideoChapterEditViewModel, ChapterItemData>(chapterItemData);

            if (editedChapterItem != null && ValidateChapter(editedChapterItem))
            {
                // 保存処理（チャプターデータにIDを設ける場合ここでID設定処理もする必要がある）
            }
        }

        /// <summary>
        /// チャプターデータを編集する。
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task Edit()
        {
            if(SelectedChapterItem == null)
            {
                _message.ShowMessage($"チャプターを選択してください。");
                return;
            }

            var editedChapterItem = await _dia.ShowDialogAsync<VideoChapterEditViewModel, ChapterItemData>(SelectedChapterItem);

            if (editedChapterItem != null && ValidateChapter(editedChapterItem))
            {
                // 保存処理（新規保存になるのを防ぐためにチャプターデータ自体にIDを設けるか、処理用に一覧取得した際に内部IDを振る必要がある）
            }
        }

        /// <summary>
        /// チャプターデータを削除する。
        /// </summary>
        [RelayCommand]
        private void Delete()
        {
            if (SelectedChapterItem == null)
            {
                _message.ShowMessage($"チャプターを選択してください。");
                return;
            }

            // 開始チャプター（00：00：0）は削除できない
            if (SelectedChapterItem.IsProtected)
            {
                _message.ShowMessage($"このチャプターは削除できません。");
                return;
            }

            // 削除処理
        }

        /// <summary>
        /// チャプターデータが登録可能か判定し、結果を返す。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ValidateChapter(ChapterItemData item)
        {
            bool result = false;

            // Start、Titleの値は既存チャプターとの重複不可
            // Startは動画の総再生時間未満であること（チャプター設定対象の動画データを取得して判定する必要がある）
            // Startは前後のチャプターのStartから５秒以上離れていること

            return result;
        }
    }
}
