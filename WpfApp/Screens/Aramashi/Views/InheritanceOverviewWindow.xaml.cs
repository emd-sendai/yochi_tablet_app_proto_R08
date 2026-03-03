using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// InheritanceOverviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InheritanceOverviewWindow : Window
    {
        /// <summary>
        /// 描画中の枠線リスト
        /// </summary>
        private List<Border> currentBorders = new List<Border>();

        public InheritanceOverviewWindow()
        {
            InitializeComponent();

            Loaded += (_, __) => UpdateAllLines();
            SizeChanged += (_, __) => UpdateAllLines();

            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is InheritanceOverviewViewModel vm)
                {
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(vm.SelectedCase))
                        {
                            if(vm.SelectedCase != string.Empty)
                            {
                                DrawBorders(vm.GetTargetsForSelectedCase());
                            }
                            else
                            {
                                DeleteBorders();
                            }
                            
                        }
                    };

                    vm.RequestClose += (_, _) => this.Close();
                }
            };
        }

        /// <summary>
        /// 描画対象オブジェクトに枠線を描画する。（stringではなく専用enumを作成して使用する形式に改良の余地あり）
        /// </summary>
        /// <param name="targetNames"></param>
        private void DrawBorders(string[] targetNames)
        {
            // 以前の枠をすべて削除
            DeleteBorders();

            foreach (string name in targetNames)
            {
                var target = FindName(name) as FrameworkElement;
                if (target == null) continue;

                var position = GetLeftTop(target);

                // デフォルトのカラーコード（赤）を設定
                var colorCode = "#D64545";

                // 配偶者の場合
                if(name == "Spouse")
                {
                    // 緑
                    colorCode = "#388E3C";
                }
                // 子の場合
                else if (name == "Children")
                {
                    // 水色
                    colorCode = "#42A5F5";
                }
                // 親の場合
                else if (name == "Parents")
                {
                    // ピンク
                    colorCode = "#E573A1";
                }
                // 兄弟姉妹の場合
                else if (name == "Siblings")
                {
                    // オレンジ
                    colorCode = "#FB8C00";
                }

                var color = (Color)ColorConverter.ConvertFromString(colorCode);
                var brush = new SolidColorBrush(color);

                Border border = new Border
                {
                    Width = target.ActualWidth,
                    Height = target.ActualHeight,
                    BorderBrush = brush,
                    BorderThickness = new Thickness(2)
                };

                Canvas.SetLeft(border, position.X);
                Canvas.SetTop(border, position.Y);

                OverlayCanvas.Children.Add(border);
                currentBorders.Add(border);
                Debug.WriteLine($"[DEBUG] 枠線を描画しました: {name}");
            }
        }

        /// <summary>
        /// 枠線の描画を初期化する。
        /// </summary>
        private void DeleteBorders()
        {
            foreach (var border in currentBorders)
                OverlayCanvas.Children.Remove(border);
            currentBorders.Clear();
            Debug.WriteLine($"[DEBUG] 描画を初期化しました");
        }

        /// <summary>
        /// 枠線の描画状態を更新する。
        /// </summary>
        private void UpdateAllLines()
        {
            var parents = GetCenter(Parents);
            var siblings = GetCenter(Siblings);
            var decedent = GetCenter(Decedent);
            var spouse = GetCenter(Spouse);
            var children = GetCenter(Children);

            var midY_parents_siblings = GetMidY(parents, siblings);
            var midX_decedent_spouse = GetMidX(decedent, spouse);

            var end_ParentsVLine = new Point(parents.X, midY_parents_siblings);
            var start_ParentsHLine = new Point(siblings.X - 2.5, midY_parents_siblings);
            var end_ParentsHLine = new Point(decedent.X + 2.5, midY_parents_siblings);
            var start_SiblingsVLine = new Point(siblings.X, midY_parents_siblings);
            var start_DecedentVLine = new Point(decedent.X, midY_parents_siblings);
            var start_ChildrenVLine = new Point(midX_decedent_spouse, decedent.Y);
            var end_ChildrenVLine = new Point(midX_decedent_spouse, children.Y);

            UpdateLine(ParentsVLine, parents, end_ParentsVLine);
            UpdateLine(ParentsHLine, start_ParentsHLine, end_ParentsHLine);
            UpdateLine(SiblingsVLine, start_SiblingsVLine, siblings);
            UpdateLine(DecedentVLine, start_DecedentVLine, decedent);
            UpdateLine(CoupleHLine, decedent, spouse);
            UpdateLine(ChildrenVLine, start_ChildrenVLine, end_ChildrenVLine);
        }

        /// <summary>
        /// 直線の描画状態を更新する。
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void UpdateLine(Line line, Point start, Point end)
        {
            line.X1 = start.X;
            line.Y1 = start.Y;
            line.X2 = end.X;
            line.Y2 = end.Y;
        }

        /// <summary>
        /// エレメントの中心座標を取得する。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private Point GetCenter(FrameworkElement element)
        {
            // elementのローカル座標における中心点を取得
            var centerLocal = new Point(element.RenderSize.Width / 2, element.RenderSize.Height / 2);

            // スクリーン座標におけるelementの中心点を取得
            var screenPoint = element.PointToScreen(centerLocal);

            // スクリーン座標をGridのローカル座標へ変換
            return FamilyTreeGrid.PointFromScreen(screenPoint);
        }

        /// <summary>
        /// エレメントの左上の座標を取得する。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private Point GetLeftTop(FrameworkElement element)
        {
            // elementの左上の点を設定
            var leftTopLocal = new Point(0,0);
            // スクリーン座標におけるelementの左上の点を取得
            var screenPoint = element.PointToScreen(leftTopLocal);
            //　スクリーン座標をGridのローカル座標へ変換
            return FamilyTreeGrid.PointFromScreen(screenPoint);
        }

        /// <summary>
        /// ２つの座標間のY軸の中心点を取得する。
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double GetMidY(Point p1, Point p2) 
        { 
            double midY = (p1.Y + p2.Y) / 2;
            return midY;
        }

        /// <summary>
        /// ２つの座標間のX軸の中心点を取得する。
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double GetMidX(Point p1, Point p2)
        {
            double midX = (p1.X + p2.X) / 2;
            return midX;
        }

        private void Border_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (DataContext is InheritanceOverviewViewModel vm)
            {
                vm.Zoom(e.DeltaManipulation.Scale.X);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is InheritanceOverviewViewModel vm)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    e.Handled = true;
                    double factor = e.Delta > 0 ? 1.1 : 1 / 1.1;
                    vm.Zoom(factor);
                }
            }
        }
    }
}
