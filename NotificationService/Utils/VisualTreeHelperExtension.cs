﻿using System.Windows;
using System.Windows.Media;

namespace NotificationService
{
    internal class VisualTreeHelperExtensions
    {
        public static T GetParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);

            if (parent == null) return null;

            var tParent = parent as T;
            if (tParent != null)
            {
                return tParent;
            }

            return GetParent<T>(parent);
        }
    }
}
