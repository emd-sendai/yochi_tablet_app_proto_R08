using System.Configuration;
using System.Data;
using System.Windows;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Interfaces;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NavigationService navigationService = default!;
        private IDialogService dialogService = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            navigationService = new NavigationService(mainWindow.MainContent);
            dialogService = new DialogService();

            // 最初に表示する画面
            navigationService.Navigate(new MenuViewModel(navigationService, dialogService));

            mainWindow.Show();
        }

    }

}
