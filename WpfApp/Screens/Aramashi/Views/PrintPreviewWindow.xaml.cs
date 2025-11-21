using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// PrintPreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintPreviewWindow : Window
    {
        public PrintPreviewWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is PrintPreviewViewModel vm)
                {
                    vm.RequestClose += (_, _) => this.Close();
                }
            };
        }

        private void DocViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var viewer = sender as DocumentViewer;
            if (viewer == null) return;

            // 下部検索バーを非表示にする
            var findBar = viewer.Template.FindName("PART_FindToolBarHost", viewer) as ContentControl;
            if (findBar != null)
                findBar.Visibility = Visibility.Collapsed;

            // 上部ツールバーの要素を編集する
            var toolBar = FindChild<ToolBar>(viewer, null);
            if (toolBar != null)
            {
                // コピーコマンドボタンだけ非表示にする
                foreach (var item in toolBar.Items)
                {
                    if (item is Button copyButton && copyButton.Command == ApplicationCommands.Copy)
                    {
                        copyButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private T? FindChild<T>(DependencyObject parent, string? name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild && (name == null || tChild.Name == name))
                    return tChild;

                var result = FindChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is PrintPreviewViewModel vm && vm.SaveAsPdfCommand.CanExecute(null))
            {
                vm.SaveAsPdfCommand.Execute(null);
            }
        }

        private void Print_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DataContext is PrintPreviewViewModel vm)
                e.CanExecute = vm.SaveAsPdfCommand.CanExecute(null);
            else
                e.CanExecute = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is PrintPreviewViewModel vm)
            {
                vm.CleanupTempFiles(); // 不要ファイル削除だけ実行
            }
        }
    }
}
