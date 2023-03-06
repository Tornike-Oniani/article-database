using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;

namespace MainLib.Views.Main
{
    public class GenericAdorner : Adorner
    {
        private readonly UIElement adorner;
        private readonly Point point;
        public GenericAdorner(UIElement targetElement, UIElement adorner, Point point) : base(targetElement)
        {
            this.adorner = adorner;
            if (adorner != null)
            {
                AddVisualChild(adorner);
            }
            this.point = point;
        }
        protected override int VisualChildrenCount
        {
            get { return adorner == null ? 0 : 1; }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (adorner != null)
            {
                adorner.Arrange(new Rect(point, adorner.DesiredSize));
            }
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && adorner != null)
            {
                return adorner;
            }
            return base.GetVisualChild(index);
        }
    }
}
