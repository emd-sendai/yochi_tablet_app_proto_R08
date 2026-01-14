using System.Windows.Controls;
using WpfApp.Interfaces;
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
                ScreenSelectViewModel => new ScreenSelectView(),
                ButtonSettingsViewModel => new ButtonSettingsView(),
                PdfDocumentSettingsViewModel => new PdfDocumentSettingsView(),
                PdfDocumentManagementViewModel => new PdfDocumentManagementView(),

                AramashiMenuTestViewModel => new AramashiMenuTestView(),

                PresentationViewerViewModel => new PresentationViewerView(),

                _ => throw new ArgumentException("Unknown ViewModel"),
            };

            view.DataContext = viewModel;
            _contentControl.Content = view;
        }

        public void GoBack()
        {
            if (_contentControl.Content is UserControl view && view.DataContext is INavigationTarget nav && nav.CreateBackViewModel != null)
            {
                Navigate(nav.CreateBackViewModel());
                return;
            }

            if (_history.Count > 0)
            {
                var previousVm = _history.Pop();
                Navigate(previousVm);
            }
        }

    }
}
