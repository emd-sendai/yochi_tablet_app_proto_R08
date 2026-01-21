using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class AppPathService : IAppPathService
    {
        public string AppRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DX_SampleApp", "R7_sampleApp");

        //public string DataRoot => Path.Combine(AppRoot, "Data");
        public string DataRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory);


        public string DataDire => Path.Combine(DataRoot, "Data");

        public string ButtonRoot => Path.Combine(DataDire, "ButtonSettings");

        public string AramashiButtonPath => Path.Combine(ButtonRoot, "aramashiButtons.json");

        public string WorksButtonPath => Path.Combine(ButtonRoot, "documentWorksButtons.json");

        public string LandownerButtonPath => Path.Combine(ButtonRoot, "documentLandownerButtons.json");

        public string SignatureDire => Path.Combine(DataDire, "Signature");

        public string LandownersPath => Path.Combine(DataDire, "landowners.json");


        public string DocumentResourcesRoot => Path.Combine(DataRoot, "DocumentResources");

        public string AramashiPdfRoot => Path.Combine(DocumentResourcesRoot, "Aramashi");

        public string AramashiPdfDire => Path.Combine(AramashiPdfRoot, "Aramashi");

        public string OthersPdfDire => Path.Combine(AramashiPdfRoot, "Others");

        public string SonshitsuPdfDire => Path.Combine(OthersPdfDire, "Sonshitsu");

        private readonly HashSet<string> _inUseFilePaths;

        public string ZeikinPdfDire => Path.Combine(OthersPdfDire, "Zeikin");

        public string DocumentPdfRoot => Path.Combine(DocumentResourcesRoot, "Documents");

        public string LandownersPdfDire => Path.Combine(DocumentPdfRoot, "Landowners");

        public string WorksPdfDire => Path.Combine(DocumentPdfRoot, "Works");


        public string MediaResourcesDire => Path.Combine(DataRoot, "MediaResources");


        public string SlideResourcesRoot => Path.Combine(DataRoot, "SlideResources");

        public string AramashiSlideDire => Path.Combine(SlideResourcesRoot, "Aramashi");

        public string OthersSlideDire => Path.Combine(SlideResourcesRoot, "Others");


        public string TempDire => Path.Combine(DataRoot, "Temp");


        public AppPathService()
        {
            _inUseFilePaths = GetInUseFilePaths();
        }


        public string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
           .TrimEnd(Path.DirectorySeparatorChar)
           .ToUpperInvariant();
        }

        private HashSet<string> GetInUseFilePaths()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省の公共用地の取得に伴う損失補償基準.pdf")),
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省の公共用地の取得に伴う損失補償基準の運用方針.pdf")),
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省損失補償取扱要領.pdf"))
            };
        }

        public bool CanDelete(string filePath)
        {
            if (_inUseFilePaths.Contains(NormalizePath(filePath)))
            {
                // 削除不可
                return false;
            }
            else
            {
                // 削除可能
                return true;
            }
        }

        public void EnsureInitialData()
        {
            // 初回コピー処理を実装
        }
    }
}
