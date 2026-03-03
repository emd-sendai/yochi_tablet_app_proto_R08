using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// VideoChapterEditWindow.xamlに対応するViewModel。（作成中）
    /// 動画チャプター編集画面での処理を行う。
    /// </summary>
    public partial class VideoChapterEditViewModel : ObservableObject, IDialogResult<ChapterItemData>
    {
        public event EventHandler? RequestClose;
        public ChapterItemData? Result { get; private set; }

        private readonly IMessageDialogService _message;

        /// <summary>
        /// 編集対象チャプターデータ
        /// </summary>
        [ObservableProperty]
        public ChapterItemData chapterItem;

        /// <summary>
        /// 開始時間テキストボックスの活性/非活性フラグ
        /// </summary>
        [ObservableProperty]
        public bool canEditStart;

        public VideoChapterEditViewModel(ChapterItemData selectedChapterItem)
        {
            _message = new MessageDialogService();

            ChapterItem = selectedChapterItem;

            // 開始チャプター（00：00：0）の場合、開始時間テキストボックスを非活性状態にする
            if (!ChapterItem.IsProtected)
            {
                CanEditStart = true;
            }
        }

        /// <summary>
        /// 編集されたチャプターデータを返して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            // 簡易バリデーション
            // 開始チャプター（00：00：0）と同じ値ではStartを登録できない
            // Startを設定できるのは0.1秒単位

            //Result = ChapterItem;
            //RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 編集状態を初期化して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
