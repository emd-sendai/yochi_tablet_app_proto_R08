using System.ComponentModel;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly IDialogService _dialogService;
        private readonly ShutdownService _shutdownService;
        //private readonly ShellViewModel _shellViewModel;
        //private readonly NavigationService _navigationService;

        /// <summary>
        /// 初期画面（メインメニュー画面）を表示する。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // DialogService の初期化
            //_dialogService = new DialogService();

            // ShutdownServiceの初期化
            _shutdownService = new ShutdownService();

            // ShellViewModelの初期化
            //_shellViewModel = new ShellViewModel();

            // NavigationServiceの初期化
            //_navigationService = new NavigationService(MainContent, _shellViewModel);

            // MainWindowのDataContextを設定
            //DataContext = _shellViewModel;

            //初期画面を設定
            //_navigationService.Navigate(new MenuViewModel(_navigationService, _dialogService));

            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// ウィンドウサイズを調整する。（コメントアウト中未使用）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double aspectRatio = e.NewSize.Width / e.NewSize.Height;

            /*
            if (aspectRatio < 1) // 縦長モード
            {
                // 例：縦長時は高さを多めに、幅は最小値以上に調整
                this.Width = Math.Max(600, e.NewSize.Width);
                this.Height = Math.Max(800, e.NewSize.Height);
            }
            else
            {
                // 横長時は標準サイズ
                this.Width = Math.Max(800, e.NewSize.Width);
                this.Height = Math.Max(600, e.NewSize.Height);
            }
            */

        }

        /// <summary>
        /// 一時保存データを初期化してメイン画面を閉じる。（×ボタン用）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            _shutdownService.Cleanup();
        }
    }
}