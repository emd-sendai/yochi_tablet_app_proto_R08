using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class LandownerEditViewModel : ObservableObject
    {
        public event Action? FocusLandownerKanaRequested;
        public event Action? FocusLandownerNameRequested;
        public event Action? ConfirmRequested;
        public event Action? CancelRequested;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private LandownerData landowner;

        [ObservableProperty]
        private bool isInsertMode;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string landownerId;

        [ObservableProperty]
        private bool isLandownerNameError = false;

        [ObservableProperty]
        private bool isLandownerKanaError = false;

        public LandownerEditViewModel(LandownerData landowner, bool isInsertMode)
        {
            Landowner = landowner;
            IsInsertMode = isInsertMode;

            if (IsInsertMode)
            {
                Title = "地権者情報新規登録";
                LandownerId = "※地権者IDは登録後自動採番されます";
            }
            else
            {
                Title = "地権者情報編集";
                LandownerId = Landowner.FormattedLandownerId;
            }
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
            IsLandownerKanaError = false;
            IsLandownerNameError = false;
        }

        private bool CheckNullOrEmpty()
        {
            bool ret = false;

            // 地権者氏名（フリガナ）
            if (string.IsNullOrEmpty(Landowner.LandownerKana))
            {
                IsLandownerKanaError = true;
                FocusLandownerKanaRequested?.Invoke();
                ret = true;
            }
            // 地権者氏名
            if (string.IsNullOrEmpty(Landowner.LandownerName))
            {
                IsLandownerNameError = true;
                if (ret == false)
                {
                    FocusLandownerNameRequested?.Invoke();
                    ret = true;
                }
            }

            return ret;
        }

        [RelayCommand]
        private void Confirm()
        {
            ClearErrorMessage();

            if (CheckNullOrEmpty())
            {
                ErrorMessage = "未入力の項目があります。";
                return;
            }

            ConfirmRequested?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            CancelRequested?.Invoke();
        }
    }
}
