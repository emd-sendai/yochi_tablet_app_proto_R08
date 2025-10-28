using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Controls;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Screens.Menu.Views;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Aramashi.Views;
using WpfApp.Screens.Documents.ViewModels;
using WpfApp.Screens.Documents.Views;

namespace WpfApp.Services
{
    public class NavigationService
    {
        private readonly ContentControl _contentControl;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void Navigate(object viewModel)
        {
            UserControl view = viewModel switch
            {
                MenuViewModel => new MenuView(),
                AramashiMenuViewModel => new AramashiMenuView(),
                SlideShellViewModel => new SlideShellView(),

                DocumentMenuViewModel => new DocumentMenuView(),

                _ => throw new ArgumentException("Unknown ViewModel"),
            };

            view.DataContext = viewModel;
            _contentControl.Content = view;
        }

    }
}
