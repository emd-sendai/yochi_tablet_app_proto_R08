namespace WpfApp.Interfaces
{
    /// <summary>
    /// アプリ内で使用するディレクトリ、ファイルのパスや状態を取得するためのインターフェース。
    /// </summary>
    public interface IAppPathService
    {
        string AppRoot { get; }

        string InitializeFlagFile { get; }

        string LogsDire { get; }
        string AppResourcesRoot { get; }

        string DataDire {  get; }
        string ButtonRoot { get; }
        string AramashiButtonPath { get; }
        string OthersButtonPath { get; }
        string WorksButtonPath { get; }
        string LandownerButtonPath { get; }
        string SignatureDire { get; }
        string LandownersPath { get; }

        string DocumentResourcesRoot { get; }
        string AramashiPdfRoot { get; }
        string AramashiPdfDire { get; }
        string OthersPdfDire { get; }
        string Others002PdfDire { get; }
        string Others003PdfDire { get; }
        string Others004PdfDire { get; }
        string Others005PdfDire { get; }
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
        string GetAramashiSlidePath(int index);
        string OthersSlideDire { get; }
        string OthersSlide001Path { get; }
        string OthersSlide002Path { get; }
        string OthersSlide003Path { get; }
        string OthersSlide004Path { get; }

        string TempDire { get; }


        string NormalizePath(string path);

        bool CanDelete(string filePath);
    }
}
