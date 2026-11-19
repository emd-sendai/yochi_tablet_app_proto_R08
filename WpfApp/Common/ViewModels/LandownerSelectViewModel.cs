using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// LandownerSelectWindow.xamlに対応するViewModel。
    /// 地権者ユーザ選択画面での処理を行う。
    /// </summary>
    public partial class LandownerSelectViewModel : ObservableObject
    {
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly ILandownerRepository _landownerRepos;

        /// <summary>
        /// 一覧表示用地権者ユーザリスト
        /// </summary>
        [ObservableProperty]
        private List<LandownerData> landowners = new();

        /// <summary>
        /// 選択された地権者ユーザ
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public event EventHandler<bool>? RequestClose;

        public LandownerSelectViewModel(ILandownerRepository landownerRepository)
        {
            _message = new MessageDialogService();
            _path = new AppPathService();
            _landownerRepos = landownerRepository;
        }

        public async void GetLandownersAsync()
        {
            Landowners = await _landownerRepos.GetAllAsync();
        }

        /// <summary>
        /// 地権者ユーザを選択して画面を閉じる。
        /// </summary>
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

        /// <summary>
        /// 選択状態を初期化して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            SelectedLandowner = null;
            RequestClose?.Invoke(this, false);
        }
    }
}
