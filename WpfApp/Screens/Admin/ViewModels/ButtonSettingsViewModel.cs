using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class ButtonSettingsViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        public ObservableCollection<ButtonConfig> Buttons { get; } = new();

        [ObservableProperty]
        private ButtonConfig? selectedButton;

        private static readonly string DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ButtonSettings");

        public ButtonSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            LoadButtonFromJson();
        }

        [RelayCommand]
        private async Task EditSelectedAsync()
        {
            // 編集対象のボタンが未選択の場合
            if (SelectedButton == null)
            {
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "ボタンを選択してください"
                };

                var dialog = new MessageDialogWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };

                dialog.ShowDialog();
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
                    var vm = new MessageDialogViewModel
                    {
                        Message = "ボタンに編集内容を適用しました"
                    };

                    var dialog = new MessageDialogWindow
                    {
                        DataContext = vm,
                        Owner = Application.Current.MainWindow
                    };

                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        // OK押下後の処理
                    }
                }
            }
        }

        private void LoadButtonFromJson()
        {
            try
            {
                string path = Path.Combine(DataFolder, "aramashiButtons.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("ボタンデータが見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if(list == null)
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
                Debug.WriteLine($"[DEBUG] ボタンデータを読み込むことができません: {ex}");
            }
        }

        private void SaveToJson()
        {
            try
            {
                string path = Path.Combine(DataFolder, "aramashiButtons.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("ボタンの保存先が見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンの保存先が見つかりません: {path}");
                    return;
                }

                var json = JsonSerializer.Serialize(Buttons, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Debug.WriteLine($"[DEBUG] 保存完了: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ボタンデータを保存できません");
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
