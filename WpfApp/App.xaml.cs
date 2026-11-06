using System.Diagnostics;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NavigationService navigationService = default!;
        private IDialogService dialogService = default!;
        private ILoggingService loggingService = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var initializer = new ResourceInitializeService();

            loggingService = new LoggingService();

            try
            {
                // LocalAppDataに必須ファイルが存在しない場合、ProgramFilesから初期ファイルをコピーする
                initializer.EnsureRequiredResources();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 必須ファイルのコピー時にエラーが発生しました");
                // ログファイルを作成する
                loggingService.LogError(ex);
            }

            // MainWindowの生成
            var mainWindow = new MainWindow();

            // ShellViewModelの生成
            var shellViewModel = new ShellViewModel();
            // NavigationServiceの生成
            navigationService = new NavigationService(mainWindow.MainContent, shellViewModel);
            // DialogServiceの生成
            dialogService = new DialogService();

            // MainWindowにShellViewModelを設定
            mainWindow.DataContext = shellViewModel;

            // 初期画面を設定
            navigationService.Navigate(new MenuViewModel(navigationService, dialogService));

            mainWindow.Show();

            // グローバル例外ハンドラ登録
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 例外をログに書き込む
            loggingService.LogError(e.Exception);

            // アプリを落とさずに続行する
            e.Handled = true;

            // ユーザーに通知
            //MessageBox.Show("予期せぬエラーが発生しました。アプリは継続します。");
        }

    }

}
