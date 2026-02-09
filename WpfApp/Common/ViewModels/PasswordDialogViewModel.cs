using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using WpfApp.Interfaces;
using WpfApp.Services;


namespace WpfApp.Common.ViewModels
{

    public partial class PasswordDialogViewModel : ObservableObject, IDialogResult<bool>
    {
        private readonly PasswordService _passwordService;

        public event EventHandler? RequestClose;
        public bool Result { get; private set; }

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public PasswordDialogViewModel()
        {
            _passwordService = new PasswordService();
        }

        [RelayCommand]
        private void Submit(PasswordBox pwBox)
        {
            if (pwBox == null) return;

            if (_passwordService.Verify(pwBox.Password))
            {
                Result = true;
                RequestClose?.Invoke(this, EventArgs.Empty);
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "パスワードが違います。";
            }
            
        }
    }
}
