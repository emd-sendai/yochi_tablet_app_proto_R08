using System.Collections.ObjectModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.Models
{
    public class TabItemData
    {
        public ObservableCollection<LandownerData>? Landowners { get; set; }

        public ObservableCollection<PdfDocumentData>? Documents { get; set; }


        public class PdfDocumentData()
        {
            public string? Id { get; set; }
            public string? DisplayName { get; set; }
            public string? FolderPath { get; set; }
        }
    }
}
