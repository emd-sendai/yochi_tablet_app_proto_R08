using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    public partial class ButtonConfig : ObservableObject
    {
        [ObservableProperty]
        private string btnId = string.Empty;

        [ObservableProperty]
        private string btnText = string.Empty;

        public string LabelText => BtnText?.Replace("\r", "").Replace("\n", " ") ?? "";

        partial void OnBtnTextChanged(string value)
        {
            OnPropertyChanged(nameof(LabelText));
        }

    }
}
