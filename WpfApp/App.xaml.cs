using System.Diagnostics;
using System.Windows;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.ViewModels;
using WpfApp.Common.Repositories;

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

        public static DatabaseService DatabaseService { get; private set; } = default!;
        public static IWorkRepository WorkRepository { get; private set; } = default!;
        public static ILandownerRepository LandownerRepository { get; private set; } = default!;
        public static IWorkLandownerRepository WorkLandownerRepository { get; private set; } = default!;
        public static IAdminPasswordRepository AdminPasswordRepository { get; private set; } = default!;
        public static DatabaseInitializeService DatabaseInitializeService { get; private set; } = default!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // LoggingServiceの生成
            loggingService = new LoggingService();

            // DatabaseServiceの生成
            DatabaseService = new DatabaseService();
            
            // Repositoryの生成
            WorkRepository = new WorkRepository(DatabaseService);
            LandownerRepository = new LandownerRepository(DatabaseService);
            WorkLandownerRepository = new WorkLandownerRepository(DatabaseService);
            AdminPasswordRepository = new AdminPasswordRepository(DatabaseService);

            // DatabaseInitializeServiceの生成
            DatabaseInitializeService = new DatabaseInitializeService(DatabaseService, WorkRepository, LandownerRepository, 
                WorkLandownerRepository, AdminPasswordRepository);
            try
            {
                // LocalAppDataにデータベースが存在しない場合、生成する（サンプルデータ含む）
                await DatabaseInitializeService.InitializeAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] データベースの初期化時にエラーが発生しました");
                // ログファイルを作成する
                loggingService.LogError(ex);
            }


            var initializer = new ResourceInitializeService();
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
