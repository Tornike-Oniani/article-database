using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;

namespace MainLib.Views.UserControls.HighlightTextBox
{
    public static class HighlightTextBoxExtension
    {
        public static void TryRemoveAdorner<T>(this HighlightTextBox element)
where T : Adorner
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(element);
            if (layer != null)
                layer.RemoveAdorners<T>(element);
        }
        public static void RemoveAdorners<T>(this AdornerLayer layer, HighlightTextBox element)
            where T : Adorner
        {
            var adorners = layer.GetAdorners(element);
            if (adorners == null) return;
            for (int i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                    layer.Remove(adorners[i]);
            }
        }
        public static void TryAddAdorner<T>(this HighlightTextBox element, Adorner adorner)
            where T : Adorner
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(element);

            if (layer != null)
                try
                {
                    layer.Add(adorner);
                }
                catch (Exception) { }
        }
        public static bool HasAdorner<T>(this AdornerLayer layer, HighlightTextBox element) where T : Adorner
        {
            var adorners = layer.GetAdorners(element);
            if (adorners == null) return false;
            for (int i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                    return true;
            }
            return false;
        }
        public static void RemoveAdorners(this AdornerLayer layer, HighlightTextBox element)
        {
            var adorners = layer.GetAdorners(element);
            if (adorners == null) return;
            foreach (Adorner remove in adorners)
                layer.Remove(remove);
        }
    }
}
