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
    /// <summary>
    /// 画面遷移処理を行う。
    /// </summary>
    public class NavigationService
    {
        private readonly ContentControl _contentControl;
        private readonly Stack<object> _history = new();

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        /// <summary>
        /// 引数のViewModelをもとに対応する画面へ遷移する。
        /// 表示中の画面は遷移前に_historyに記録される。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <exception cref="ArgumentException">
        /// ViewModelが存在しない場合。
        /// </exception>
        public void Navigate(object viewModel)
        {
            if (_contentControl.Content is UserControl currentView &&
                currentView.DataContext != null)
            {
                _history.Push(currentView.DataContext);
            }

            UserControl view = viewModel switch
            {
                /* メインメニュー */
                MenuViewModel => new MenuView(),
                // 補償のあらまし
                AramashiMenuViewModel => new AramashiMenuView(),
                PresentationViewerViewModel => new PresentationViewerView(),
                // 各種資料
                DocumentMenuViewModel => new DocumentMenuView(),

                /* 管理メニュー */
                // 表示設定（共通画面）
                DisplaySettingsViewModel => new DisplaySettingsView(),
                ScreenSelectViewModel => new ScreenSelectView(),
                SubCategorySelectViewModel => new SubCategorySelectView(),
                // 表示設定（ボタンの名称）
                ButtonSettingsViewModel => new ButtonSettingsView(),
                // 表示設定（PDF資料）
                PdfDocumentSettingsViewModel => new PdfDocumentSettingsView(),
                PdfDocumentManagementViewModel => new PdfDocumentManagementView(),
                // 表示設定（動画）
                VideoFileSettingsViewModel => new VideoFileSettingsView(),
                VideoChapterSettingsViewModel => new VideoChapterSettingsView(),
                //　管理資料
                AdminDocumentViewModel => new AdminDocumentView(),

                _ => throw new ArgumentException("Unknown ViewModel"),
            };

            view.DataContext = viewModel;
            _contentControl.Content = view;
        }

        /// <summary>
        /// １つ前に表示していた画面に遷移する。
        /// 遷移対象が指定されている場合はその画面に遷移する。
        /// </summary>
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
