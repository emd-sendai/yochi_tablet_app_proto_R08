using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Helpers
{
    public static class DisableDoubleClickBehavior
    {
        public static bool GetIsEnabled(Button obj)
    => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(Button obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DisableDoubleClickBehavior),
                new PropertyMetadata(false, OnChanged));

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button btn) return;

            if ((bool)e.NewValue)
                btn.Click += OnClick;
            else
                btn.Click -= OnClick;
        }

        private static async void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            // すでに無効化中なら無視（多重発火防止）
            if (!btn.IsHitTestVisible)
                return;

            try
            {
                btn.IsHitTestVisible = false;  // 見た目変えずにクリック不能化
                await Task.Delay(300);         // 禁止時間
            }
            finally
            {
                btn.IsHitTestVisible = true;   // 元に戻す
            }
        }
    }
}
