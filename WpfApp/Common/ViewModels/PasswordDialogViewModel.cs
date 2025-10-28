using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfApp.Services;
using WpfApp.Interfaces;


namespace WpfApp.Common.ViewModels
{

    public partial class PasswordDialogViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        [ObservableProperty]
        private string password = string.Empty;

        [RelayCommand]
        private void Submit(PasswordBox pwBox)
        {
            Result = pwBox.Password;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
