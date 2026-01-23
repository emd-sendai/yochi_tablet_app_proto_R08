namespace WpfApp.Interfaces
{
    public interface IAppPathService
    {
        string AppRoot { get; }

        string InitializeFlagFile { get; }

        string LogsDire { get; }
        string AppResourcesRoot { get; }

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

        string MediaResourcesRoot { get; }
        string MediaAramashiRoot { get; }
        string MediaMeinAramashiDire { get; }
        string MediaMeinAramashiChapterPath { get; }
        string MediaPartsAramashiDire { get; }
        string MediaPartsOthersDire { get; }

        string SlideResourcesRoot { get; }
        string AramashiSlideDire { get; }
        string OthersSlideDire { get; }

        string TempDire { get; }


        string NormalizePath(string path);

        bool CanDelete(string filePath);
    }
}
