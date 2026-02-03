using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
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

        [ObservableProperty]
        private ObservableCollection<ChapterItemData> chapterItems;

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

        [RelayCommand]
        private async Task Add()
        {
            var chapterItemData = new ChapterItemData(new ChapterData());

            var editedChapterItem = await _dia.ShowDialogAsync<VideoChapterEditViewModel, ChapterItemData>(chapterItemData);

            if (editedChapterItem != null)
            {
                // 保存処理
            }
        }

        [RelayCommand]
        private async Task Edit()
        {
            if(SelectedChapterItem == null)
            {
                _message.ShowMessage($"チャプターを選択してください。");
                return;
            }

            var editedChapterItem = await _dia.ShowDialogAsync<VideoChapterEditViewModel, ChapterItemData>(SelectedChapterItem);

            if (editedChapterItem != null)
            {
                // 保存処理（新規保存になるのを防ぐために内部Idを設けた方がいいかも）
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (SelectedChapterItem == null)
            {
                _message.ShowMessage($"チャプターを選択してください。");
                return;
            }

            if (SelectedChapterItem.IsProtected)
            {
                _message.ShowMessage($"このチャプターは削除できません。");
                return;
            }

            // 削除処理
        }

        public void ValidateChapter(ChapterItemData item)
        {

        }
    }
}
