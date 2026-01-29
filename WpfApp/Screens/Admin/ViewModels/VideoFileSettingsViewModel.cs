using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class VideoFileSettingsViewModel : ObservableObject, INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly ILoggingService _log;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        public Func<object>? CreateBackViewModel => () => new SubCategorySelectViewModel(_nav, _dia, DisplaySettingsCategory.VideoSettings);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        [ObservableProperty]
        private ObservableCollection<VideoData> videoDatas;

        public VideoFileSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _log = new LoggingService();
            _message = new MessageDialogService();
            _path = new AppPathService();
        }

        [RelayCommand]
        private void ToggleDrawer()
        {
            IsLeftDrawerOpen = !IsLeftDrawerOpen;
        }

        [RelayCommand]
        private void GoToDisplaySetttings()
        {
            _nav.Navigate(new DisplaySettingsViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAdminDocument()
        {
            _nav.Navigate(new AdminDocumentViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoBack()
        {
            _nav.GoBack();
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));

        }
    }
}
