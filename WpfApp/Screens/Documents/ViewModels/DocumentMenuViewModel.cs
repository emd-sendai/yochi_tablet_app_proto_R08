using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Documents.ViewModels
{
    public partial class DocumentMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        [ObservableProperty]
        private ObservableCollection<LandownerData>? landowners;

        [ObservableProperty]
        private LandownerData? selectedLandowner;


        public bool IsLandownerSelected => SelectedLandowner != null;


        public DocumentMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        }

        partial void OnSelectedLandownerChanged(LandownerData? oldValue, LandownerData? newValue)
        {
            // SelectedLandowner が変わったら IsLandownerSelected も通知
            OnPropertyChanged(nameof(IsLandownerSelected));
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void OpenPasswordDialog()
        {
            // パスワードダイアログを生成
            var pwVm = new PasswordDialogViewModel();
            var pwDialog = new PasswordDialogWindow
            {
                DataContext = pwVm,
                Owner = Application.Current.MainWindow
            };

            // ShowDialog() は true/false/null を返す
            if (pwDialog.ShowDialog() == true)
            {
                // 入力されたパスワードを参照
                if (pwVm.Result == "0000")
                {
                    // パスワードが正解の場合、選択ウィンドウを開く
                    var lanWin = new LandownerSelectWindow
                    {
                        Owner = Application.Current.MainWindow
                    };

                    if (lanWin.ShowDialog() == true)
                    {
                        // 選択された地権者ユーザを取得して画面に反映
                        SelectedLandowner = lanWin.SelectedLandowner;

                        // 地権者ユーザが選択されている場合（未選択の状態でキャンセルボタン押下時以外の場合）
                        if (SelectedLandowner != null)
                        {
                            //メッセージウィンドウを表示
                            var vm = new MessageDialogViewModel
                            {
                                Message = $"{SelectedLandowner.LandownerName} 様の資料を表示します"
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
                else
                {
                    //地権者ユーザの選択を初期化
                    SelectedLandowner = null;

                    //メッセージウィンドウを表示
                    var vm = new MessageDialogViewModel
                    {
                        Message = "パスワードが違います"
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

    }
}
