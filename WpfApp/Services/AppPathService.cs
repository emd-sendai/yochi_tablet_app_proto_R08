using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class AppPathService : IAppPathService
    {
        public string AppRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyCompany", "MyApp");

        //public string DataRoot => Path.Combine(AppRoot, "Data");
        public string DataRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory);


        public string DataDire => Path.Combine(DataRoot, "Data");

        public string ButtonDire => Path.Combine(DataDire, "ButtonSettings");

        public string SignatureDire => Path.Combine(DataDire, "Signature");


        public string DocumentResourcesRoot => Path.Combine(DataRoot, "DocumentResources");

        public string AramashiPdfRoot => Path.Combine(DocumentResourcesRoot, "Aramashi");

        public string AramashiPdfDire => Path.Combine(AramashiPdfRoot, "Aramashi");

        public string OthersPdfDire => Path.Combine(AramashiPdfRoot, "Others");

        public string SonshitsuPdfDire => Path.Combine(OthersPdfDire, "Sonshitsu");

        public string ZeikinPdfDire => Path.Combine(OthersPdfDire, "Zeikin");

        public string DocumentPdfRoot => Path.Combine(DocumentResourcesRoot, "Documents");

        public string LandownersPdfDire => Path.Combine(DocumentPdfRoot, "Landowners");

        public string WorksPdfDire => Path.Combine(DocumentPdfRoot, "Works");


        public string MediaResourcesDire => Path.Combine(DataRoot, "MediaResources");


        public string SlideResourcesRoot => Path.Combine(DataRoot, "SlideResources");

        public string AramashiSlideDire => Path.Combine(SlideResourcesRoot, "Aramashi");

        public string OthersSlideDire => Path.Combine(SlideResourcesRoot, "Others");


        public string TempDire => Path.Combine(DataRoot, "Temp");


        public void EnsureInitialData()
        {
            // 初回コピー処理を実装
        }
    }
}
