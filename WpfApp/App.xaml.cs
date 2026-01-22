using System.Diagnostics;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NavigationService navigationService = default!;
        private IDialogService dialogService = default!;
        private IAppPathService appPathService = default!;
        private ILoggingService loggingService = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            appPathService = new AppPathService();
            var initializer = new ResourceInitializeService(appPathService);

            loggingService = new LoggingService();

            try
            {
                // 初回起動時のみコンテンツディレクトリのコピーを行う
                //initializer.InitializeIfNeeded();
            }
            catch (Exception ex)
            {
                // MSIX ではログ出力推奨（メモ）
                // アプリを落とさない
                Debug.WriteLine($"[ERROR] コンテンツディレクトリのコピー時にエラーが発生しました");
                loggingService.LogError(ex);
            }

            var mainWindow = new MainWindow();
            navigationService = new NavigationService(mainWindow.MainContent);
            dialogService = new DialogService();

            // 最初に表示する画面
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
