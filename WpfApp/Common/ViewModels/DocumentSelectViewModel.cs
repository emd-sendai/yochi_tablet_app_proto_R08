using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// DocumentSelectWindow.xamlに対応するViewModel。
    /// PDF資料選択画面での処理を行う。
    /// </summary>
    public partial class DocumentSelectViewModel : ObservableObject
    {
        private readonly IFileService _file;

        /// <summary>
        /// ファイルリスト
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<FileData> files;

        /// <summary>
        /// 選択されたファイル（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private FileData? selectedFile;

        public event EventHandler? RequestClose;
        public event EventHandler<string>? ShowMessageRequested;

        public DocumentSelectViewModel(List<FileData> list)
        {
            _file = new FileService();
            Files = new ObservableCollection<FileData>(list);
        }

        /// <summary>
        /// 選択されたPDF資料を開く。
        /// </summary>
        [RelayCommand]
        private void Select()
        {
            if (SelectedFile == null) { return; }

            // ファイルが存在しない場合
            if (!File.Exists(SelectedFile.FilePath))
            {
                Debug.WriteLine($"[ERROR] ファイルが存在しません {SelectedFile.FilePath}");
                ShowMessageRequested?.Invoke(this, "ファイルが存在しません。");
            }
            else
            {
                try
                {
                    Debug.WriteLine($"[[DEBUG] ファイルを開きます {SelectedFile.FilePath}");
                    _file.OpenFile(SelectedFile);
                }
                catch (FileNotFoundException ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルが存在しません {SelectedFile.FilePath}");
                    ShowMessageRequested?.Invoke(this, "ファイルが存在しません。");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルにアクセス権限がありません {SelectedFile.FilePath}");
                    ShowMessageRequested?.Invoke(this, "ファイルにアクセス権限がありません。");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルを開くことができません {SelectedFile.FilePath}");
                    ShowMessageRequested?.Invoke(this, "ファイルを開くことができません。");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルの表示時に予期せぬエラーが発生しました {SelectedFile.FilePath}");
                    ShowMessageRequested?.Invoke(this, "ファイルの表示時に予期せぬエラーが発生しました。");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// 画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
