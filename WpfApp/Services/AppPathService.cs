using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// アプリ内で使用するディレクトリ、ファイルのパスや状態を取得する。
    /// </summary>
    public class AppPathService : IAppPathService
    {
        public string AppRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyCompany", "SampleApp");

        public string InitializeFlagFile => Path.Combine(AppRoot, ".initialized");


        //public string DataRoot => Path.Combine(AppRoot, "Logs");
        public string LogsDire => Path.Combine(AppResourcesRoot, "Logs");

        //public string AppResourcesRoot => Path.Combine(AppRoot, "AppResources");
        public string AppResourcesRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory);


        public string DataDire => Path.Combine(AppResourcesRoot, "Data");

        public string ButtonRoot => Path.Combine(DataDire, "ButtonSettings");

        public string AramashiButtonPath => Path.Combine(ButtonRoot, "aramashiButtons.json");

        public string OthersButtonPath => Path.Combine(ButtonRoot, "othersButtons.json");

        public string WorksButtonPath => Path.Combine(ButtonRoot, "documentWorksButtons.json");

        public string LandownerButtonPath => Path.Combine(ButtonRoot, "documentLandownerButtons.json");

        public string SignatureDire => Path.Combine(DataDire, "Signature");

        public string LandownersPath => Path.Combine(DataDire, "landowners.json");


        public string DocumentResourcesRoot => Path.Combine(AppResourcesRoot, "DocumentResources");

        public string AramashiPdfRoot => Path.Combine(DocumentResourcesRoot, "Aramashi");

        public string AramashiPdfDire => Path.Combine(AramashiPdfRoot, "Aramashi");

        public string OthersPdfDire => Path.Combine(AramashiPdfRoot, "Others");

        public string SonshitsuPdfDire => Path.Combine(OthersPdfDire, "Sonshitsu");

        /// <summary>
        /// 他画面のリンクから直接参照されているパス。
        /// </summary>
        private readonly HashSet<string> _linkedPathsInOtherView;

        public string ZeikinPdfDire => Path.Combine(OthersPdfDire, "Zeikin");

        public string DocumentPdfRoot => Path.Combine(DocumentResourcesRoot, "Documents");

        public string LandownersPdfDire => Path.Combine(DocumentPdfRoot, "Landowners");

        public string WorksPdfDire => Path.Combine(DocumentPdfRoot, "Works");


        public string MediaResourcesRoot => Path.Combine(AppResourcesRoot, "MediaResources");

        public string MediaAramashiRoot => Path.Combine(MediaResourcesRoot, "Aramashi");

        public string MediaMeinAramashiDire => Path.Combine(MediaAramashiRoot, "Main");

        public string MediaMeinAramashiChapterPath => Path.Combine(MediaMeinAramashiDire, "chapters.json");

        public string MediaPartsAramashiDire => Path.Combine(MediaAramashiRoot, "Parts", "Aramashi");

        public string MediaPartsOthersDire => Path.Combine(MediaAramashiRoot, "Parts", "Others");


        public string SlideResourcesRoot => Path.Combine(AppResourcesRoot, "SlideResources");

        public string AramashiSlideDire => Path.Combine(SlideResourcesRoot, "Aramashi");

        public string OthersSlideDire => Path.Combine(SlideResourcesRoot, "Others");


        public string TempDire => Path.Combine(AppResourcesRoot, "Temp");


        public AppPathService()
        {
            _linkedPathsInOtherView = GetlinkedPathsInOtherView();
        }

        /// <summary>
        /// pathの形式を標準化する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
           .TrimEnd(Path.DirectorySeparatorChar)
           .ToUpperInvariant();
        }

        /// <summary>
        /// 他画面のリンクから直接参照されているパスを取得する。
        /// </summary>
        /// <returns></returns>
        private HashSet<string> GetlinkedPathsInOtherView()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省の公共用地の取得に伴う損失補償基準.pdf")),
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省の公共用地の取得に伴う損失補償基準の運用方針.pdf")),
                NormalizePath(Path.Combine(SonshitsuPdfDire, "国土交通省損失補償取扱要領.pdf"))
            };
        }

        /// <summary>
        /// 指定されたfilePathが削除可能か判定し、その結果を返す。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool CanDelete(string filePath)
        {
            // 他画面から直接参照されている場合
            if (_linkedPathsInOtherView.Contains(NormalizePath(filePath)))
            {
                // 削除不可
                return false;
            }
            // それ以外の場合
            else
            {
                // 削除可能
                return true;
            }
        }
    }
}
