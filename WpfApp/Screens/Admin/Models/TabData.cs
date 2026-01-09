using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.Models
{
    public class TabItemData
    {
        public ObservableCollection<LandownerData>? Landowners { get; set; }

        public ObservableCollection<DocumentData>? Documents { get; set; }


        public class DocumentData()
        {
            public string? DisplayName { get; set; }

            public string? FolderPath { get; set; }
        }
    }
}
