using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class EditButtonViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        private const double MaxFullWidthLength = 10;

        [ObservableProperty] private string line1;
        [ObservableProperty] private string line2;
        [ObservableProperty] private string line3;

        [ObservableProperty] private double line1Remaining;
        [ObservableProperty] private double line2Remaining;
        [ObservableProperty] private double line3Remaining;

        public string PreviewText => string.Join("\n", new[] { Line1, Line2, Line3 }.Where(l => !string.IsNullOrWhiteSpace(l)));

        public EditButtonViewModel(string text)
        {
            var lines = (text ?? "").Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            Line1 = TruncateByFullWidth(lines.ElementAtOrDefault(0) ?? "");
            Line2 = TruncateByFullWidth(lines.ElementAtOrDefault(1) ?? "");
            Line3 = TruncateByFullWidth(lines.ElementAtOrDefault(2) ?? "");

            UpdateRemaining();
        }

        partial void OnLine1Changed(string value)
        {
            Line1 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        partial void OnLine2Changed(string value)
        {
            Line2 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        partial void OnLine3Changed(string value)
        {
            Line3 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        private void UpdateRemaining()
        {
            Line1Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line1));
            Line2Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line2));
            Line3Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line3));
        }

        private static string TruncateByFullWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            double count = 0;
            int index = 0;

            while (index < text.Length)
            {
                char c = text[index];
                double add = IsFullWidth(c) ? 1 : 0.5;

                if (count + add > MaxFullWidthLength)
                    break;

                count += add;
                index++;
            }

            return text.Substring(0, index);
        }

        private static double CountFullWidthChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            double count = 0;
            foreach (char c in text)
                count += IsFullWidth(c) ? 1 : 0.5;

            return count;
        }

        private static bool IsFullWidth(char c)
        {
            return c > 0xFF; // 全角判定（日本語・全角記号）
        }

        [RelayCommand]
        private void Confirm()
        {
            Result = string.Join("\n", new[] { Line1, Line2, Line3 }.Where(l => !string.IsNullOrWhiteSpace(l)));
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
