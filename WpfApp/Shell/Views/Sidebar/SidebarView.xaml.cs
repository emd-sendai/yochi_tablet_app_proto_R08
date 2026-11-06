using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp.Shell.Views.Sidebar
{
    /// <summary>
    /// SidebarView.xaml の相互作用ロジック
    /// </summary>
    public partial class SidebarView : UserControl
    {
        public SidebarView()
        {
            InitializeComponent();
        }

        public object SidebarContent
        {
            get => GetValue(SidebarContentProperty);
            set => SetValue(SidebarContentProperty, value);
        }

        public static readonly DependencyProperty SidebarContentProperty =
            DependencyProperty.Register(
                nameof(SidebarContent),
                typeof(object),
                typeof(SidebarView),
                new PropertyMetadata(null));
    }
}
