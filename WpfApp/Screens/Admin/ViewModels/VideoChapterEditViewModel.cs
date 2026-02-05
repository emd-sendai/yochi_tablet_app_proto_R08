using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class VideoChapterEditViewModel : ObservableObject, IDialogResult<ChapterItemData>
    {
        public event EventHandler? RequestClose;
        public ChapterItemData? Result { get; private set; }

        private readonly IMessageDialogService _message;

        [ObservableProperty]
        public ChapterItemData chapterItem;

        [ObservableProperty]
        public bool canEditStart; // 開始時間テキストボックス非表示用

        public VideoChapterEditViewModel(ChapterItemData selectedChapterItem)
        {
            _message = new MessageDialogService();

            ChapterItem = selectedChapterItem;

            if (!ChapterItem.IsProtected)
            {
                CanEditStart = true;
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            //Result = ChapterItem;
            //RequestClose?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
