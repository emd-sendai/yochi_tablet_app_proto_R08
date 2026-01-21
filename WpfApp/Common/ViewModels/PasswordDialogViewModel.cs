using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using WpfApp.Interfaces;


namespace WpfApp.Common.ViewModels
{

    public partial class PasswordDialogViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        [RelayCommand]
        private void Submit(PasswordBox pwBox)
        {
            if (pwBox == null) return;

            Result = pwBox.Password;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
