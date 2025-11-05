using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Common.ViewModels;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class AramashiMenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public AramashiMenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void OpenVideoPlayer()
        {
            // 非モーダル表示
            _dia.ShowWindow<VideoPlayerViewModel> ("AramashiSample.mp4");
        }

        [RelayCommand]
        private void GoToSection1()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(0);
        }

        [RelayCommand]
        private void GoToSection2()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(1);
        }

        [RelayCommand]
        private void GoToSection3()
        {
            var slideVM = new SlideShellViewModel("Aramashi", _nav, _dia);
            _nav.Navigate(slideVM);
            slideVM.GoToSlide(2);
        }

    }
}
