using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    /// <summary>
    /// InheritanceOverviewView.xamlに対応するViewModel。
    /// 相続の概要画面での処理を行う。
    /// </summary>
    public partial class InheritanceOverviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        /// <summary>
        /// 選択された描画ケース
        /// </summary>
        [ObservableProperty]
        private string selectedCase;

        /// <summary>
        /// 描画ケースごとの描画対象オブジェクトを格納したディクショナリ（stringではなく専用enumを作成して使用する形式に改良の余地あり）
        /// </summary>
        public Dictionary<string, string[]> Targets { get; } = new Dictionary<string, string[]>
        {
            { "spouseOnly", new[] { "Spouse" } },
            { "spouseAndChidren", new[] { "Spouse", "Children" } },
            { "spouseAndParents", new[] { "Spouse", "Parents" } },
            { "spouseAndSiblings", new[] { "Spouse", "Siblings" } }
        };

        [ObservableProperty]
        private double scale = 1.3;

        public double MinScale { get; } = 0.1;
        public double MaxScale { get; } = 5.0;

        public InheritanceOverviewViewModel() 
        {
            SelectedCase = string.Empty;
        }

        /// <summary>
        /// 描画ケースを選択する。
        /// </summary>
        /// <param name="_selectedCase"></param>
        [RelayCommand]
        private void SelectCase(string _selectedCase)
        {
            if (SelectedCase == _selectedCase)
            {
                SelectedCase = string.Empty;
                Debug.WriteLine($"[DEBUG] 描画ケースの選択が解除されました");
            }
            else
            {
                SelectedCase = _selectedCase;
                Debug.WriteLine($"[DEBUG] 描画ケースが選択されました: {SelectedCase}");
            }
        }

        /// <summary>
        /// ディクショナリから描画対象オブジェクトを取得して返す。
        /// </summary>
        /// <returns></returns>
        public string[] GetTargetsForSelectedCase()
        {
            if (string.IsNullOrEmpty(SelectedCase)) 
            {
                Debug.WriteLine($"[DEBUG] 描画ケースが空です: {SelectedCase}");
                return new string[0]; 
            }

            string[] ret;

            if (Targets.TryGetValue(SelectedCase, out var targets)) {
                ret = targets.ToArray();

                if (ret.Length > 0) 
                {
                    Debug.WriteLine($"[DEBUG] 描画対象オブジェクト: " + string.Join(", ", ret));
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] 描画対象オブジェクトが設定されていません");
                }
            }
            else 
            {
                ret = new string[0];
                Debug.WriteLine($"[DEBUG] キーが存在しません: {SelectedCase}");
            }

            return ret;
        }

        /// <summary>
        /// 表示倍率を変更する。
        /// </summary>
        /// <param name="factor"></param>
        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        /// <summary>
        /// 表示を拡大する。
        /// </summary>
        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        /// <summary>
        /// 表示を縮小する。
        /// </summary>
        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        /// <summary>
        /// 画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
