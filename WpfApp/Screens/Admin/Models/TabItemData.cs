using System.Collections.ObjectModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.Models
{
    /// <summary>
    /// Tabに表示するTabItemを格納する。
    /// </summary>
    public class TabItemData
    {
        /// <summary>
        /// 地権者ユーザデータリスト（地権者個別資料のPDF資料の表示時のみ使用）
        /// </summary>
        public ObservableCollection<LandownerData>? Landowners { get; set; }

        /// <summary>
        /// PDF資料格納項目データリスト
        /// </summary>
        public ObservableCollection<PdfDocumentData>? Documents { get; set; }


        public class PdfDocumentData()
        {
            /// <summary>
            /// 対応する機能のボタンID（地権者個別資料のPDF資料の表示時のみ使用）
            /// </summary>
            public string? Id { get; set; }
            public string? DisplayName { get; set; }
            public string? FolderPath { get; set; }
        }
    }
}
