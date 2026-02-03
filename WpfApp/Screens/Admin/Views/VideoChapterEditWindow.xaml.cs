using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// VideoChapterEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoChapterEditWindow : Window
    {
        private static readonly Regex _numberOnly = new(@"^\d+$");

        public VideoChapterEditWindow()
        {
            InitializeComponent();
        }

        private void NumberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_numberOnly.IsMatch(e.Text);
        }

        private void NumberOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Text)) return;

            var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            if (string.IsNullOrEmpty(text) || !_numberOnly.IsMatch(text))
                e.CancelCommand();
        }
    }
}
