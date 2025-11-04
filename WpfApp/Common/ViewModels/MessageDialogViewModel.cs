using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Common.ViewModels
{
    public partial class MessageDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string message = "";

        public event EventHandler? RequestClose;

        [RelayCommand]
        private void Ok()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
