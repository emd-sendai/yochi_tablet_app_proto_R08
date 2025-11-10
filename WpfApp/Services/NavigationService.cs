using System.Windows.Controls;
using WpfApp.Screens.Admin.ViewModels;
using WpfApp.Screens.Admin.Views;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Aramashi.Views;
using WpfApp.Screens.Documents.ViewModels;
using WpfApp.Screens.Documents.Views;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Screens.Menu.Views;

namespace WpfApp.Services
{
    public class NavigationService
    {
        private readonly ContentControl _contentControl;
        private readonly Stack<object> _history = new();

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void Navigate(object viewModel)
        {
            if (_contentControl.Content is UserControl currentView &&
                currentView.DataContext != null)
            {
                _history.Push(currentView.DataContext);
            }

            UserControl view = viewModel switch
            {
                MenuViewModel => new MenuView(),
                AramashiMenuViewModel => new AramashiMenuView(),
                SlideShellViewModel => new SlideShellView(),
                DocumentMenuViewModel => new DocumentMenuView(),

                DisplaySettingsViewModel => new DisplaySettingsView(),
                ButtonSettingsViewModel => new ButtonSettingsView(),

                AramashiMenuTestViewModel => new AramashiMenuTestView(),
                DocumentMenu2ViewModel => new DocumentMenu2View(),

                _ => throw new ArgumentException("Unknown ViewModel"),
            };

            view.DataContext = viewModel;
            _contentControl.Content = view;
        }

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                var previousVm = _history.Pop();
                Navigate(previousVm);
            }
        }

    }
}
