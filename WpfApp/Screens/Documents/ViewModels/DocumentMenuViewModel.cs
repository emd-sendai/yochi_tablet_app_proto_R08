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
    /// <summary>
    /// DocumentMenuView.xamlに対応するViewModel。
    /// 各種資料画面での処理を行う。
    /// </summary>
    public partial class DocumentMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        /// <summary>
        /// ボタンの名称表示用ボタンデータリスト（事業全般資料）
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? worksButtons;

        /// <summary>
        /// ボタンの名称表示用ボタンデータリスト（地権者個別資料）
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? landownerButtons;

        /// <summary>
        /// 選択中の地権者ユーザ。
        /// nullの場合は未選択状態を表す。
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        private bool isPasswordCorrect;

        public bool IsLandownerSelected => SelectedLandowner != null;


        public DocumentMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _message = new MessageDialogService();
            _path = new AppPathService();

            WorksButtons = LoadButtonFromJson(_path.WorksButtonPath);
            LandownerButtons = LoadButtonFromJson(_path.LandownerButtonPath);
        }

        /// <summary>
        /// ボタンに表示する名称を取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ObservableCollection<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new ObservableCollection<ButtonConfig>();

            if (!File.Exists(path))
            {
                _message.ShowMessage("ボタンの名称を表示できません。");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if(list == null || list.Count <= 0)
                {
                    _message.ShowMessage("ボタンの名称を表示できません。");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                }
                else
                {
                    list = list.OrderBy(x => x.BtnId).ToList();

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
            // SelectedLandowner の値が変更された際に IsLandownerSelected に通知
            OnPropertyChanged(nameof(IsLandownerSelected));
        }

        /// <summary>
        /// メインメニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// パスワード画面を表示する。
        /// パスワードが正しい場合に地権者ユーザ選択画面を表示する。
        /// </summary>
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
                    if (pwVm.Result)
                    {
                        isPasswordCorrect = true;
                        // 地権者ユーザ選択画面を表示する
                        OpenLandownerSelectWindow();
                    }
                }
            }
        }

        /// <summary>
        /// 地権者ユーザ選択画面を表示する。
        /// 選択中の地権者ユーザをSelectedLandownerに代入する。
        /// </summary>
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
                    _message.ShowMessage($"{SelectedLandowner.LandownerName} 様の資料を表示します");
                }
            }
        }

        /// <summary>
        /// 事業全般資料の各項目に対応するPDF資料選択画面を表示する。
        /// </summary>
        /// <param name="folderName"></param>
        [RelayCommand]
        private void OpenWorksPdfDocument(string folderName)
        {
            var folderPath = Path.Combine(_path.WorksPdfDire, folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("PDFファイルが存在しません");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }

        /// <summary>
        /// 地権者個別資料の各項目に対応するPDF資料選択画面を表示する。
        /// </summary>
        /// <param name="folderName"></param>
        [RelayCommand]
        private void OpenLandownerPdfDocument(string folderName)
        {
            if (SelectedLandowner == null) return;
            if (string.IsNullOrWhiteSpace(SelectedLandowner.LandownerId)) return;

            var folderPath = Path.Combine(_path.LandownersPdfDire, SelectedLandowner.LandownerId, folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("PDFファイルが存在しません");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }
    }
}
