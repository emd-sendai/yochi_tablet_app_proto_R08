using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WpfApp.Common.Models;

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
            if (SelectedLandowner == null)
            {
                // 未選択の場合はメッセージを表示する
                System.Windows.MessageBox.Show("地権者ユーザを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RequestClose?.Invoke(this, true);
        }

        [RelayCommand]
        private void Cancel()
        {
            SelectedLandowner = null;
            RequestClose?.Invoke(this, false);
        }
    }
}
