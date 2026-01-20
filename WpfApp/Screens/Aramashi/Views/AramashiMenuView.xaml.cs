using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// AramashiMenuView.xaml の相互作用ロジック
    /// </summary>
    public partial class AramashiMenuView : UserControl
    {
        public AramashiMenuView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            SizeChanged += (s, e) => UpdateLine();

            foreach (var btn in new[] { BtnL1, BtnL5, BtnR5, BtnR1 })
            {
                btn.SizeChanged += (s, e) => UpdateLine();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 最初のレイアウト確定後に呼び出し
            Dispatcher.BeginInvoke(() => UpdateLine(),
                System.Windows.Threading.DispatcherPriority.Loaded);

            // ウィンドウサイズが変わった場合にも再描画する
            if (Window.GetWindow(this) is Window win)
            {
                win.SizeChanged += (s, ev) => UpdateLine();
            }
        }

        /// <summary>
        /// レイアウト更新（引数なし呼び出し用）
        /// </summary>
        private void UpdateLine()
        {
            if (!IsLoaded || LineCanvas == null) return;

            LineCanvas.Width = MainGrid.ActualWidth;
            LineCanvas.Height = MainGrid.ActualHeight;

            // U字型の角座標
            Point p1 = BtnL1.TransformToVisual(LineCanvas)
                .Transform(new Point(BtnL1.ActualWidth / 2, BtnL1.ActualHeight / 2));
            Point p2 = BtnL5.TransformToVisual(LineCanvas)
                .Transform(new Point(BtnL5.ActualWidth / 2, BtnL5.ActualHeight / 2));
            Point p3 = BtnR5.TransformToVisual(LineCanvas)
                .Transform(new Point(BtnR5.ActualWidth / 2, BtnR5.ActualHeight / 2));
            Point p4 = BtnR1.TransformToVisual(LineCanvas)
                .Transform(new Point(BtnR1.ActualWidth / 2, BtnR1.ActualHeight + 10));

            // U字型ライン
            ULine.Points.Clear();
            ULine.Points.Add(p1);
            ULine.Points.Add(p2);
            ULine.Points.Add(p3);
            ULine.Points.Add(p4);

            Point p5 = BtnR1.TransformToVisual(LineCanvas)
                .Transform(new Point(BtnR1.ActualWidth / 2, BtnR1.ActualHeight));

            // 矢印描画
            DrawArrow(p3, p5, 10, 40);
        }

        /// <summary>
        /// 矢印を描画
        /// </summary>
        private void DrawArrow(Point start, Point end, double length, double width)
        {
            Vector direction = start - end;
            if (direction.Length == 0) return;

            direction.Normalize();
            Vector perpendicular = new Vector(-direction.Y, direction.X);

            Point p1 = end;
            Point p2 = end + direction * length + perpendicular * width / 2;
            Point p3 = end + direction * length - perpendicular * width / 2;

            PathFigure figure = new PathFigure { StartPoint = p1, IsClosed = true };
            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(p3, true));

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            Arrow.Data = geometry;
        }
    }
}
