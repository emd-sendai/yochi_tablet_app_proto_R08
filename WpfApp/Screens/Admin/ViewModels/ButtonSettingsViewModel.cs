using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// ButtonSettingsView.xamlに対応するViewModel。
    /// ボタンの名称設定画面での処理を行う。（現在は補償のあらまし画面の用地補償の手順のボタンの名称のみ編集可能）
    /// </summary>
    public partial class ButtonSettingsViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        /// <summary>
        /// 一覧表示用ボタンデータリスト
        /// </summary>
        public ObservableCollection<ButtonConfig> Buttons { get; } = new();

        /// <summary>
        /// 選択されたボタンデータ
        /// </summary>
        [ObservableProperty]
        private ButtonConfig? selectedButton;

        public ButtonSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
            _path = new AppPathService();

            LoadButtonFromJson();
        }

        /// <summary>
        /// 選択されたボタンデータを渡してボタンの名称編集画面を開く。
        /// ボタンの名称編集画面から返ってきた入力値をボタンの名称として保存する。
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task EditSelectedAsync()
        {
            // 編集対象のボタンが未選択の場合
            if (SelectedButton == null)
            {
                //メッセージウィンドウを表示
                _message.ShowMessage("ボタンを選択してください");
            }
            else
            {
                // DialogService を使ってモーダル Window を開く
                var editedText = await _dia.ShowDialogAsync<EditButtonViewModel, string>(SelectedButton.BtnText);

                if (!string.IsNullOrWhiteSpace(editedText))
                {
                    SelectedButton.BtnText = editedText;
                    SaveToJson();

                    //メッセージウィンドウを表示
                    _message.ShowMessage("ボタンに編集内容を適用しました");
                }
            }
        }

        /// <summary>
        /// 一覧表示用ボタンデータリストを取得する。
        /// </summary>
        private void LoadButtonFromJson()
        {
            try
            {
                string path = _path.AramashiButtonPath;

                if (!File.Exists(path))
                {
                    _message.ShowMessage("ボタンデータが見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if(list == null)
                {
                    _message.ShowMessage("ボタンデータが空です");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                    return;
                }

                var sortedList = list.OrderBy(x => x.BtnId).ToList();
                Debug.WriteLine($"[DEBUG] 読み込み完了: {path} 件数 = {sortedList.Count}");
                Buttons.Clear();
                foreach (var btn in sortedList)
                    Buttons.Add(btn);
            }
            catch (Exception ex)
            {
                _message.ShowMessage("ボタンデータを読み込むことができません");
                Debug.WriteLine($"[DEBUG] ボタンデータを読み込むことができません: {ex}");
            }
        }

        /// <summary>
        /// ボタンデータを保存する。
        /// </summary>
        private void SaveToJson()
        {
            try
            {
                string path = _path.AramashiButtonPath;

                if (!File.Exists(path))
                {
                    _message.ShowMessage("ボタンの保存先が見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンの保存先が見つかりません: {path}");
                    return;
                }

                var json = JsonSerializer.Serialize(Buttons, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Debug.WriteLine($"[DEBUG] 保存完了: {path}");
            }
            catch (Exception ex)
            {
                _message.ShowMessage("ボタンデータを保存できません");
                Debug.WriteLine($"[DEBUG] ボタンデータを保存できません: {ex}");
            }
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
    }
}
