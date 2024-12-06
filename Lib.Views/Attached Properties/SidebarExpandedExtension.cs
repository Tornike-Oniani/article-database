using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lib.Views.Attached_Properties
{
    public static class SidebarExpandedExtension
    {
        public static bool GetIsExpanded(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsExpandedProperty);
        }

        public static void SetIsExpanded(DependencyObject obj, bool value)
        {
            obj.SetValue(IsExpandedProperty, value);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.RegisterAttached
            (
                "IsExpanded",
                typeof(bool),
                typeof(SidebarExpandedExtension),
                new UIPropertyMetadata(false)
            );
    }
}
