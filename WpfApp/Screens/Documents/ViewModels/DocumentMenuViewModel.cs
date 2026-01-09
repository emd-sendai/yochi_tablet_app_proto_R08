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

namespace WpfApp.Screens.Documents.ViewModels
{
    public partial class DocumentMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;

        private static readonly string BaseDire = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ButtonDataFolder = Path.Combine(BaseDire, "Data", "ButtonSettings");
        private static readonly string WorksButtonPath = Path.Combine(ButtonDataFolder, "documentWorksButtons.json");
        private static readonly string LandownerButtonPath = Path.Combine(ButtonDataFolder, "documentLandownerButtons.json");
        private static readonly string DocumentDataFolder = Path.Combine(BaseDire, "DocumentResources", "Documents");

        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? worksButtons;

        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? landownerButtons;

        [ObservableProperty]
        private ObservableCollection<LandownerData>? landowners;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        private bool isPasswordCorrect;

        public bool IsLandownerSelected => SelectedLandowner != null;


        public DocumentMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();

            WorksButtons = LoadButtonFromJson(WorksButtonPath);
            LandownerButtons = LoadButtonFromJson(LandownerButtonPath);
        }

        private ObservableCollection<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new ObservableCollection<ButtonConfig>();

            if (!File.Exists(path))
            {
                MessageBox.Show("ボタンの名称を表示できません");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if(list == null || list.Count <= 0)
                {
                    MessageBox.Show("ボタンの名称を表示できません");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                }
                else
                {
                    foreach (var btn in list)
                    {
                        buttons.Add(btn);
                    }
                    Debug.WriteLine($"[DEBUG] ボタンデータ読み込み完了: {path} 件数 = {buttons.Count}");
                }
            }

            return buttons;
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
            // パスワードが照合済の場合
            if(isPasswordCorrect == true)
            {
                OpenLandownerSelectWindow();
            }
            // パスワードが照合されていない場合
            else
            {
                // パスワードダイアログを生成
                var pwVm = new PasswordDialogViewModel();
                var pwDialog = new PasswordDialogWindow
                {
                    DataContext = pwVm,
                    Owner = Application.Current.MainWindow
                };

                // パスワード入力後にOKボタンが押下された場合
                if (pwDialog.ShowDialog() == true)
                {
                    // パスワードが正しい場合
                    if (pwVm.Result == "0000")
                    {
                        isPasswordCorrect = true;
                        
                        // 地権者ユーザ選択ウィンドウを開く
                        OpenLandownerSelectWindow();
                    }
                    // パスワードが誤っている場合
                    else
                    {
                        isPasswordCorrect = false;

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

        private void OpenLandownerSelectWindow()
        {
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

        [RelayCommand]
        private void OpenWorksPdfDocument(string folderName)
        {
            var folderPath = Path.Combine(DocumentDataFolder, "Works", folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "PDFファイルが存在しません"
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
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }

        [RelayCommand]
        private void OpenLandownerPdfDocument(string folderName)
        {
            if (SelectedLandowner == null) return;
            if (string.IsNullOrWhiteSpace(SelectedLandowner.LandownerId)) return;

            var folderPath = Path.Combine(DocumentDataFolder, "Landowners", SelectedLandowner.LandownerId, folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "PDFファイルが存在しません"
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
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }
    }
}
