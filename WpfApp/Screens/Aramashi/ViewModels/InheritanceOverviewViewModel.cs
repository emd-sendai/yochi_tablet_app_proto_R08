using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class InheritanceOverviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        [ObservableProperty]
        private string selectedCase;

        public Dictionary<string, string[]> Targets { get; } = new Dictionary<string, string[]>
        {
            { "spouseOnly", new[] { "Spouse" } },
            { "spouseAndChidren", new[] { "Spouse", "Children" } },
            { "spouseAndParents", new[] { "Spouse", "Parents" } },
            { "spouseAndSiblings", new[] { "Spouse", "Siblings" } }
        };

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 0.1;
        public double MaxScale { get; } = 5.0;

        public InheritanceOverviewViewModel() 
        {
            SelectedCase = string.Empty;
        }

        [RelayCommand]
        private void SelectCase(string _selectedCase)
        {
            if (SelectedCase == _selectedCase)
            {
                SelectedCase = string.Empty;
                Debug.WriteLine($"描画ケースの選択が解除されました");
            }
            else
            {
                SelectedCase = _selectedCase;
                Debug.WriteLine($"描画ケースが選択されました: {SelectedCase}");
            }
        }

        public string[] GetTargetsForSelectedCase()
        {
            if (string.IsNullOrEmpty(SelectedCase)) 
            {
                Debug.WriteLine($"描画ケースが空です: {SelectedCase}");
                return new string[0]; 
            }

            string[] ret;

            if (Targets.TryGetValue(SelectedCase, out var targets)) {
                ret = targets.ToArray();

                if (ret.Length > 0) 
                {
                    Debug.WriteLine($"描画対象オブジェクト: " + string.Join(", ", ret));
                }
                else
                {
                    Debug.WriteLine($"描画対象オブジェクトが設定されていません");
                }
            }
            else 
            {
                ret = new string[0];
                Debug.WriteLine($"キーが存在しません: {SelectedCase}");
            }

            return ret;
        }

        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
