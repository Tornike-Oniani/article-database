using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lib.Styles.Attached_Properties
{
    public static class TextBoxHelper
    {
        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(TextBoxHelper), new UIPropertyMetadata(string.Empty));

        public static bool GetIsFloating(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFloatingProperty);
        }

        public static void SetIsFloating(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFloatingProperty, value);
        }

        public static readonly DependencyProperty IsFloatingProperty =
            DependencyProperty.RegisterAttached("IsFloating", typeof(bool), typeof(TextBoxHelper), new UIPropertyMetadata(false));
    }
}
