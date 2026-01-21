using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    public partial class LandownerSelectViewModel : ObservableObject
    {
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        [ObservableProperty]
        private ObservableCollection<LandownerData>? landowners;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public event EventHandler<bool>? RequestClose;

        public LandownerSelectViewModel()
        {
            _message = new MessageDialogService();
            _path = new AppPathService();

            LoadLandownersFromJson();
        }

        private void LoadLandownersFromJson()
        {
            try 
            { 
                string path = _path.LandownersPath;

                if (!File.Exists(path))
                {
                    _message.ShowMessage("地権者データが見つかりません");
                    Debug.WriteLine($"[DEBUG] 地権者データが見つかりません: {path}");
                } 
                    
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<LandownerData>>(json);

                Landowners = new ObservableCollection<LandownerData>(list ?? new List<LandownerData>());
            }
            catch (Exception ex)
            {
                _message.ShowMessage("地権者データを読み込むことができません");
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
                _message.ShowMessage("地権者ユーザを選択してください");
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
