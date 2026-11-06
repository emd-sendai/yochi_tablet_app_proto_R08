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
using WpfApp.Shell.Models;
using WpfApp.Shell.ViewModels;

namespace WpfApp.Services
{
    /// <summary>
    /// 画面遷移処理を行うサービスクラス。
    /// </summary>
    public class NavigationService
    {
        private readonly ContentControl _contentControl;
        private readonly ShellViewModel _shellViewModel;
        private readonly Stack<object> _history = new();

        public NavigationService(ContentControl contentControl , ShellViewModel shellViewModel)
        {
            _contentControl = contentControl;
            _shellViewModel = shellViewModel;
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
            if (viewModel is not IShellLayout layout)
            {
                throw new ArgumentNullException("IShellLayoutを実装していないViewModelです。", nameof(viewModel));
            }

            // _historyに記録
            if (_contentControl.Content is UserControl currentView &&
                currentView.DataContext != null)
            {
                _history.Push(currentView.DataContext);
            }
            // 表示を切り替える
            ShowView(viewModel);
        }

        /// <summary>
        /// １つ前に表示していた画面に遷移する。
        /// 遷移対象が指定されている場合はその画面に遷移する。
        /// </summary>
        public void GoBack()
        {
            // 遷移対象が指定されている場合
            if (_contentControl.Content is UserControl currentView &&
                 currentView.DataContext is INavigationTarget nav &&
                nav.CreateBackViewModel != null)
            {
                // 指定の画面に遷移する
                var backViewModel = nav.CreateBackViewModel();
                ShowView(backViewModel);
                return;
            }
            // _historyの記録が1つ前の画面に遷移する
            GoBackFromHistory();
        }

        private void GoBackFromHistory()
        {
            if (_history.Count == 0)
            {
                return;
            }
            var previousVm = _history.Pop();
            ShowView(previousVm);
        }

        private void ShowView(object viewModel)
        {
            if (viewModel is not IShellLayout layout)
            {
                throw new ArgumentNullException("IShellLayoutを実装していないViewModelです。", nameof(viewModel));
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
            // ヘッダー、サイドバーを更新
            _shellViewModel.UpdateLayout(layout, viewModel);
            // メインを更新
            _contentControl.Content = view;
        }

    }
}
