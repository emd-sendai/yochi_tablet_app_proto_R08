using System.IO;

namespace WpfApp.Common.Models
{
    public class PdfData
    {
        public string FileName { get; }

        public string FullFileName { get; }

        public string FullPath { get; }

        public PdfData(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileNameWithoutExtension(fullPath);
            FullFileName = Path.GetFileName(fullPath);
        }
    }
}
