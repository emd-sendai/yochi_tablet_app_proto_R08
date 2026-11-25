using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace WpfApp.Common.Models
{
    public partial class FileData : ObservableObject
    {
        /// <summary>
        /// ファイルパス
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FileName))]
        [NotifyPropertyChangedFor(nameof(FileNameWithoutExtension))]
        [NotifyPropertyChangedFor(nameof(FileExtension))]
        [NotifyPropertyChangedFor(nameof(DirePath))]
        private string filePath = string.Empty;

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>
        /// ファイル名（拡張子なし）
        /// </summary>
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// 拡張子
        /// </summary>
        public string FileExtension => Path.GetExtension(FilePath);

        /// <summary>
        /// ディレクトリパス
        /// </summary>
        public string DirePath => Path.GetDirectoryName(FilePath) ?? string.Empty;
    }
}
