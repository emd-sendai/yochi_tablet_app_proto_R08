using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// VideoFileSettingsView.xamlに対応するViewModel。
    /// 動画ファイル（mp4）の差し替え処理を行う。
    /// </summary>
    public partial class VideoFileSettingsViewModel : ObservableObject, INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly ILoggingService _log;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IVideoService _video;

        public Func<object>? CreateBackViewModel => () => new SubCategorySelectViewModel(_nav, _dia, DisplaySettingsCategory.VideoSettings);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        [ObservableProperty]
        private ObservableCollection<VideoData> videoDatas;

        [ObservableProperty]
        private VideoData? selectedVideoData;

        public VideoFileSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _log = new LoggingService();
            _message = new MessageDialogService();
            _path = new AppPathService();
            _video = new VideoService();

            VideoDatas = GetVideoDatas();
        }

        /// <summary>
        /// 動画ファイルを設定できる項目リストを取得する。
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<VideoData> GetVideoDatas()
        {
            var datas = new ObservableCollection<VideoData>();

            // 統合版動画
            var video = new VideoData();
            video.DirectoryPath = _video.GetVideoFilePath(_path.MediaMeinAramashiDire);
            video.LinkedPartLabel = "補償のあらまし（統合版）";
            datas.Add(video);

            // 用地補償の手順の個別動画
            var aramashiButtons = LoadAramashiButtonFromJson();
            foreach (var button in aramashiButtons)
            {
                datas.Add(GetVideoDataFromButtonData(true, button));
            }

            // その他の個別動画
            var othersButtons = LoadOthersButtonFromJson();
            foreach (var button in othersButtons)
            {
                // 代替地と税金等の場合
                if (button.BtnId == "001" || button.BtnId == "002")
                {
                    datas.Add(GetVideoDataFromButtonData(false, button));
                }
            }

            return datas;
        }

        /// <summary>
        /// ButtonDataから項目の表示名と対応するDirectoryパスを取得し、VideoDataに格納する。
        /// </summary>
        /// <param name="isAramashi"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        private VideoData GetVideoDataFromButtonData(bool isAramashi, ButtonConfig button)
        {
            string direPath;

            if (isAramashi)
            {
                direPath = Path.Combine(_path.MediaPartsAramashiDire, button.BtnId);
            }
            else
            {
                direPath = Path.Combine(_path.MediaPartsOthersDire, button.BtnId);
            }

            var video = new VideoData();
            video.DirectoryPath = direPath;
            video.LinkedPartLabel = button.LabelText;

            return video;
        }

        /// <summary>
        /// 補償のあらましの用地補償の手順のボタン設定データを取得する。
        /// </summary>
        /// <returns></returns>
        private List<ButtonConfig> LoadAramashiButtonFromJson()
        {
            var buttons = new List<ButtonConfig>();
            try
            {
                string path = _path.AramashiButtonPath;

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return buttons;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list == null)
                {
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                    return buttons;
                }

                buttons = list.OrderBy(x => x.BtnId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] ボタンデータを読み込むことができません: {ex}");
            }

            return buttons;
        }

        /// <summary>
        /// 補償のあらましのその他のボタン設定データを取得する。
        /// </summary>
        /// <returns></returns>
        private List<ButtonConfig> LoadOthersButtonFromJson()
        {
            var buttons = new List<ButtonConfig>();
            try
            {
                string path = _path.OthersButtonPath;

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return buttons;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list == null)
                {
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                    return buttons;
                }

                buttons = list.OrderBy(x => x.BtnId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] ボタンデータを読み込むことができません: {ex}");
            }

            return buttons;
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
        /// 選択された項目にローカルフォルダから選択した動画ファイル（mp4）を設定する。
        /// </summary>
        [RelayCommand]
        private void Change()
        {
            // 編集対象の項目が未選択の場合
            if (SelectedVideoData == null)
            {
                //メッセージウィンドウを表示
                _message.ShowMessage("項目を選択してください。");
                Debug.WriteLine($"[DEBUG] SelectedVideoDataがNULLです");
                return;
            }

            // 説明用メッセージダイアログを表示
            _message.ShowMessage("ローカルフォルダからmp4ファイルを選択してください。（単一選択）");
            var selectedFile = ShowSelectVideoFileDialog();

            if (selectedFile == null || selectedFile == string.Empty) { return; }

            try
            {
                _video.ChangeVideoFile(SelectedVideoData, selectedFile);

                // 統合版動画が変更された場合
                if (SelectedVideoData.DirectoryPath == _path.MediaMeinAramashiDire)
                {
                    // 統合版動画のチャプター設定を初期化する
                    _video.ClearChapter(_path.MediaMeinAramashiChapterPath);
                }

                _message.ShowConfirm($"以下の項目に動画ファイルを設定しました。\n「{SelectedVideoData.LinkedPartLabel}」");
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[ERROR] 使用中か読み取り専用のためファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _message.ShowConfirm("ファイルが開かれているため、上書きできません。" +
                    "\nファイルを閉じてから、もう一度お試しください。");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[ERROR] アクセス権が不足しているためファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _message.ShowConfirm("アクセス権が不足しているため、ファイルを上書きできません。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _log.LogError(ex);
                _message.ShowConfirm("ファイルを上書きできません。\n管理者へお問い合わせください。");
            }

        }

        /// <summary>
        /// mp4ファイル選択ダイアログを表示する。（単一選択）
        /// </summary>
        /// <returns></returns>
        private string ShowSelectVideoFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                Title = "mp4 ファイルを選択",
                Filter = "mp4 ファイル (*.mp4)|*.mp4",
                Multiselect = false
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // 選択されたmp4ファイルを返す
                return dlg.FileName;
            }
            else
            {
                // 空の文字列を返す
                return string.Empty;
            }
        }
    }
}
