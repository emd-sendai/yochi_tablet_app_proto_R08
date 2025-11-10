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

        public ObservableCollection<ButtonData> Buttons { get; } = new();

        [ObservableProperty]
        private ButtonData selectedButton = new();

        public ButtonSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            LoadButtonFromJson();
        }

        private static string GetButtonFilePath()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, "aramashiButtons.json");
        }

        [RelayCommand]
        private async Task EditSelectedAsync()
        {
            if (SelectedButton == null) return;

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

        private void LoadButtonFromJson()
        {
            try
            {
                string path = GetButtonFilePath();
                Debug.WriteLine($"[DEBUG] 読み込みパス: {path}");

                if (!File.Exists(path))
                {
                    MessageBox.Show("ボタンデータが見つかりません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが見つかりません: {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonData>>(json) ?? new List<ButtonData>();
                Debug.WriteLine($"[DEBUG] 読み込み完了: {path} 件数={list.Count}");

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
                string path = GetButtonFilePath();
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
