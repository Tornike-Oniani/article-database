using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainLib.Views.UserControls.HighlightTextBox
{
    public class HighlightTextBox : TextBox
    {
        public List<string> HighlightWords
        {
            get { return (List<string>)GetValue(HighlightWordsProperty); }
            set { SetValue(HighlightWordsProperty, value); }
        }

        public static readonly DependencyProperty HighlightWordsProperty =
            DependencyProperty.Register("HighlightWords", typeof(List<string>), typeof(HighlightTextBox), new FrameworkPropertyMetadata(new List<string>(), new PropertyChangedCallback(HighlightWordsChanged)));

        private static void HighlightWordsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            HighlightTextBox tb = (HighlightTextBox)sender;
            tb.ApplyHighlights();
        }

        public static Brush GetHighlightColor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightColorProperty);
        }

        public static void SetHighlightColor(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightColorProperty, value);
        }

        public static readonly DependencyProperty HighlightColorProperty =
            DependencyProperty.RegisterAttached("HighlightColor", typeof(Brush), typeof(HighlightTextBox),
                new PropertyMetadata(Brushes.Yellow, new PropertyChangedCallback(HighlightWordsChanged)));

        public HighlightTextBox() : base()
        {
            this.Loaded += HighlightTextBox_Loaded;
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            ApplyHighlights();
        }
        private void HighlightTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyHighlights();
        }

        static HighlightTextBox()
        {
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void ApplyHighlights()
        {
            this.TryRemoveAdorner<GenericAdorner>();

            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            foreach (string word in HighlightWords)
            {
                for (int index = 0; ; index += word.Length)
                {
                    index = Text.IndexOf(word, index, StringComparison.CurrentCultureIgnoreCase);
                    if (index == -1)
                        break;
                    indexes.Add(new Tuple<int, int>(index, word.Length));
                }
            }

            if (indexes.Count == 0) { return; }
            List<Tuple<int, int>> newIndexes = new List<Tuple<int, int>>();
            int i = 0;
            Tuple<int, int> currentItem;
            Tuple<int, int> nextItem;
            currentItem = indexes[i];

            // Remove overlaps
            while (true)
            {
                if (i == indexes.Count - 1)
                {
                    newIndexes.Add(currentItem);
                    break;
                }

                nextItem = indexes[i + 1];

                // Is overlap
                if (currentItem.Item1 + currentItem.Item2 > nextItem.Item1)
                {
                    if (nextItem.Item1 + nextItem.Item2 < currentItem.Item1 + currentItem.Item2)
                    {
                        //newIndexes.Add(currentItem);
                    }
                    else
                    {
                        currentItem = new Tuple<int, int>(currentItem.Item1, nextItem.Item1 + nextItem.Item2 - currentItem.Item1);
                    }
                    i++;
                }
                else
                {
                    newIndexes.Add(currentItem);
                    i++;
                    currentItem = indexes[i];
                }
            }

            foreach (Tuple<int, int> indexLength in newIndexes)
            {
                if (!String.IsNullOrEmpty(Text))
                {
                    if (base.ActualHeight != 0 && base.ActualWidth != 0)
                    {
                        Rect rect = base.GetRectFromCharacterIndex(indexLength.Item1);
                        Rect backRect = base.GetRectFromCharacterIndex(indexLength.Item1 + indexLength.Item2 - 1, true);
                        this.TryAddAdorner<GenericAdorner>(new GenericAdorner(this, new Rectangle()
                        { Height = rect.Height, Width = backRect.X - rect.X, Fill = GetHighlightColor(this), Opacity = 0.5, IsHitTestVisible = false }, new Point(rect.X, rect.Y)));
                    }
                }
            }

        }
    }
}
