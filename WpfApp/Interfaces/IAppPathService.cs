namespace WpfApp.Interfaces
{
    public interface IAppPathService
    {
        string AppRoot { get; }
        string DataRoot { get; }

        string DataDire {  get; }
        string ButtonRoot { get; }
        string AramashiButtonPath { get; }
        string WorksButtonPath { get; }
        string LandownerButtonPath { get; }
        string SignatureDire { get; }
        string LandownersPath { get; }

        string DocumentResourcesRoot { get; }
        string AramashiPdfRoot { get; }
        string AramashiPdfDire { get; }
        string OthersPdfDire { get; }
        string SonshitsuPdfDire { get; }
        string ZeikinPdfDire { get; }
        string DocumentPdfRoot { get; }
        string LandownersPdfDire { get; }
        string WorksPdfDire { get; }

        string MediaResourcesDire { get; }

        string SlideResourcesRoot { get; }
        string AramashiSlideDire { get; }
        string OthersSlideDire { get; }

        string TempDire { get; }


        string NormalizePath(string path);

        bool CanDelete(string filePath);

        void EnsureInitialData();
    }
}
