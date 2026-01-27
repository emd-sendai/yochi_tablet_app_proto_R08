using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// 初期配置用のディレクトリ、ファイルのパスを取得する。
    /// </summary>
    public class ResourcePathService : IResourcePathService
    {
        public string Root => AppDomain.CurrentDomain.BaseDirectory;

        public string DataDire => Path.Combine(Root, "Data");

        public string ButtonRoot => Path.Combine(DataDire, "ButtonSettings");

        public string AramashiButtonPath => Path.Combine(ButtonRoot, "aramashiButtons.json");

        public string OthersButtonPath => Path.Combine(ButtonRoot, "othersButtons.json");

        public string WorksButtonPath => Path.Combine(ButtonRoot, "documentWorksButtons.json");

        public string LandownerButtonPath => Path.Combine(ButtonRoot, "documentLandownerButtons.json");

        public string SignatureDire => Path.Combine(DataDire, "Signature");

        public string LandownersPath => Path.Combine(DataDire, "landowners.json");


        public string DocumentResourcesRoot => Path.Combine(Root, "DocumentResources");

        public string AramashiPdfRoot => Path.Combine(DocumentResourcesRoot, "Aramashi");

        public string AramashiPdfDire => Path.Combine(AramashiPdfRoot, "Aramashi");

        public string OthersPdfDire => Path.Combine(AramashiPdfRoot, "Others");

        public string SonshitsuPdfDire => Path.Combine(OthersPdfDire, "Sonshitsu");

        public string ZeikinPdfDire => Path.Combine(OthersPdfDire, "Zeikin");

        public string DocumentPdfRoot => Path.Combine(DocumentResourcesRoot, "Documents");

        public string LandownersPdfDire => Path.Combine(DocumentPdfRoot, "Landowners");

        public string WorksPdfDire => Path.Combine(DocumentPdfRoot, "Works");


        public string MediaResourcesRoot => Path.Combine(Root, "MediaResources");

        public string MediaAramashiRoot => Path.Combine(MediaResourcesRoot, "Aramashi");

        public string MediaMeinAramashiDire => Path.Combine(MediaAramashiRoot, "Main");

        public string MediaMeinAramashiChapterPath => Path.Combine(MediaMeinAramashiDire, "chapters.json");

        public string MediaPartsAramashiDire => Path.Combine(MediaAramashiRoot, "Parts", "Aramashi");

        public string MediaPartsOthersDire => Path.Combine(MediaAramashiRoot, "Parts", "Others");


        public string SlideResourcesRoot => Path.Combine(Root, "SlideResources");

        public string AramashiSlideDire => Path.Combine(SlideResourcesRoot, "Aramashi");

        public string OthersSlideDire => Path.Combine(SlideResourcesRoot, "Others");
    }
}
