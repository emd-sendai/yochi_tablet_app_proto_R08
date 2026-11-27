using WpfApp.Common.Models;
using WpfApp.Screens.Admin.Enums;

namespace WpfApp.Screens.Admin.Models
{
    public class FileAddErrors
    {
        public Dictionary<FileAddFailureReason, List<FileData>> Errors { get; } = new();

        public void Add(FileAddFailureReason reason, string path)
        {
            if (!Errors.TryGetValue(reason, out var files))
            {
                files = new List<FileData>();
                Errors.Add(reason, files);
            }
            var file = new FileData() { FilePath = path };
            files.Add(file);
        }
    }
}
