namespace WpfApp.Interfaces
{
    /// <summary>
    /// 初期配置用のディレクトリ、ファイルのパスを取得するためのインターフェース。
    /// </summary>
    public interface IResourcePathService
    {
        string Root { get; }

        string DataDire { get; }
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
        string OthersSlideDire { get; }
    }
}
