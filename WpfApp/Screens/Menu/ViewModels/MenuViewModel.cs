using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Aramashi.Views;
using WpfApp.Screens.Documents.ViewModels;
using WpfApp.Screens.Documents.Views;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Screens.Menu.Views;
using WpfApp.Services;

namespace WpfApp.Screens.Menu.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public MenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToDocumentMenu()
        {
            _nav.Navigate(new DocumentMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAramashiMenuTest()
        {
            _nav.Navigate(new AramashiMenuTestViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void Exit()
        {
            //タスクの解放

            Application.Current.Shutdown();
        }

    }
}
