using System.ComponentModel;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly NavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ShutdownService _shutdownService;

        /// <summary>
        /// 初期画面を表示する
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // NavigationServiceにContentControl渡す
            _navigationService = new NavigationService(MainContent);

            // DialogService の初期化
            _dialogService = new DialogService();

            // ShutdownServiceの初期化
            _shutdownService = new ShutdownService();

            // 初期画面を設定
            _navigationService.Navigate(new MenuViewModel(_navigationService, _dialogService));

            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// ウィンドウサイズを調整する
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

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            _shutdownService.Cleanup();
        }
    }
}