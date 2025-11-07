using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.Views;

namespace WpfApp.Common.ViewModels
{
    public partial class LandownerSelectViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<LandownerData>? landowners;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public event EventHandler<bool>? RequestClose;

        public LandownerSelectViewModel()
        {
            LoadLandownersFromJson();
        }

        private void LoadLandownersFromJson()
        {
            try 
            { 
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/landowners.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("地権者データが見つかりません");
                    Debug.WriteLine($"[DEBUG] 地権者データが見つかりません: {path}");
                } 
                    
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<LandownerData>>(json);

                landowners = new ObservableCollection<LandownerData>(list ?? new List<LandownerData>());
            }
            catch (Exception ex)
            {
                MessageBox.Show("地権者データを読み込むことができません");
                Debug.WriteLine($"[DEBUG] 地権者データを読み込むことができません: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Select()
        {
            // 地権者ユーザが選択されていない場合
            if (SelectedLandowner == null)
            {
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "地権者ユーザを選択してください"
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
            else
            {
                RequestClose?.Invoke(this, true);
            }
                
        }

        [RelayCommand]
        private void Cancel()
        {
            SelectedLandowner = null;
            RequestClose?.Invoke(this, false);
        }
    }
}
