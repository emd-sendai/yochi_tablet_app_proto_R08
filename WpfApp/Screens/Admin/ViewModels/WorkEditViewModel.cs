using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class WorkEditViewModel : ObservableObject
    {
        public event Action? FocusWorkNameRequested;
        public event Action? ConfirmRequested;
        public event Action? CancelRequested;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private WorkData work;

        [ObservableProperty]
        private bool isInsertMode;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string workId;

        [ObservableProperty]
        private bool isWorkNameError = false;

        public WorkEditViewModel(WorkData work, bool isInsertMode)
        {
            Work = work;
            IsInsertMode = isInsertMode;

            if (IsInsertMode)
            {
                Title = "事業情報新規登録";
                WorkId = "※事業IDは登録後自動採番されます";
            }
            else
            {
                Title = "事業情報編集";
                WorkId = work.FormattedWorkId;
            }
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
            IsWorkNameError = false;
        }

        [RelayCommand]
        private void Confirm()
        {
            ClearErrorMessage();

            if (string.IsNullOrEmpty(Work.WorkName))
            {
                IsWorkNameError = true;
                FocusWorkNameRequested?.Invoke();
                ErrorMessage = "事業名を入力してください。";
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
