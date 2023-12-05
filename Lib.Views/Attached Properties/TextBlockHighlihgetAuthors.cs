using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lib.Views.Attached_Properties
{
    public static class TextBlockHighlighterAuthors
    {
        public static string GetSelection(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectionProperty);
        }

        public static void SetSelection(DependencyObject obj, string value)
        {
            obj.SetValue(SelectionProperty, value);
        }

        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.RegisterAttached("Selection", typeof(string), typeof(TextBlockHighlighterAuthors),
                new PropertyMetadata(new PropertyChangedCallback(SelectText)));

        private static void SelectText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool wordBreakMode = (bool)d.GetValue(WordBreakModeProperty);
            if (wordBreakMode)
            {
                WordBreakSelect(d);
                return;
            }

            DefaultSelect(d);
        }

        private static void DefaultSelect(DependencyObject d)
        {
            if (d == null) return;
            if (!(d is TextBlock)) throw new InvalidOperationException("Only valid for TextBlock");

            TextBlock txtBlock = d as TextBlock;
            string text = txtBlock.Text;
            if (string.IsNullOrEmpty(text)) return;

            string highlightText = (string)d.GetValue(SelectionProperty);
            if (string.IsNullOrEmpty(highlightText)) return;

            int index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0) return;

            Brush selectionColor = (Brush)d.GetValue(HighlightColorProperty);
            Brush forecolor = (Brush)d.GetValue(ForecolorProperty);

            txtBlock.Inlines.Clear();
            while (true)
            {
                txtBlock.Inlines.AddRange(new Inline[] {
                    new Run(text.Substring(0, index)),
                    new Run(text.Substring(index, highlightText.Length)) {Background = selectionColor,
                        Foreground = forecolor}
                });

                text = text.Substring(index + highlightText.Length);
                index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);

                if (index < 0)
                {
                    txtBlock.Inlines.Add(new Run(text));
                    break;
                }
            }
        }
        private static void WordBreakSelect(DependencyObject d)
        {
            if (d == null) return;
            if (!(d is TextBlock)) throw new InvalidOperationException("Only valid for TextBlock");

            TextBlock txtBlock = d as TextBlock;
            string text = txtBlock.Text;
            if (string.IsNullOrEmpty(text)) return;

            string highlight = (string)d.GetValue(SelectionProperty);
            if (string.IsNullOrEmpty(highlight)) return;
            string[] highlightAuthors = highlight.Trim().Split(',');
            if (highlightAuthors.Length <= 0) return;

            Brush selectionColor = (Brush)d.GetValue(HighlightColorProperty);
            Brush forecolor = (Brush)d.GetValue(ForecolorProperty);

            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();

            string[] names;
            Regex regex;
            foreach (string highlightText in highlightAuthors)
            {
                names = highlightText.Split(' ');
                if (names.Length > 1)
                {
                    regex = new Regex($@"(?i)\b{names[0][0]}[\w\.]*\s.*{names[names.Length - 1]}");
                }
                else
                {
                    regex = new Regex($"(?i){names[0]}");
                }
                Console.WriteLine(regex.Match(text).Index);
                Console.WriteLine(regex.Match(text).Value);
                indexes.Add(new Tuple<int, int>(regex.Match(text).Index, regex.Match(text).Value.Length));
                
                //for (int index = 0; ; index += highlightText.Length)
                //{
                //    index = text.IndexOf(highlightText, index, StringComparison.CurrentCultureIgnoreCase);
                //    if (index == -1)
                //        break;
                //    indexes.Add(new Tuple<int, int>(index, highlightText.Length));
                //}
            }

            if (indexes.Count <= 0) return;

            indexes.Sort((x, y) => x.Item1.CompareTo(y.Item1));

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
                        //newIndexes.Add(new Tuple<int, int>(currentItem.Item1, nextItem.Item1 + nextItem.Item2 - currentItem.Item1));
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

            txtBlock.Inlines.Clear();

            Tuple<int, int> last = newIndexes.Last();
            int current = 0;
            foreach (Tuple<int, int> indexLength in newIndexes)
            {
                txtBlock.Inlines.AddRange(new Inline[] {
                    new Run(text.Substring(current, indexLength.Item1 - current)),
                    new Run(text.Substring(indexLength.Item1, indexLength.Item2)) {Background = selectionColor,
                        Foreground = forecolor}
                });

                current = indexLength.Item1 + indexLength.Item2;
                string test = text.Substring(current);
            }

            txtBlock.Inlines.Add(new Run(text.Substring(last.Item1 + last.Item2)));
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
            DependencyProperty.RegisterAttached("HighlightColor", typeof(Brush), typeof(TextBlockHighlighterAuthors),
                new PropertyMetadata(Brushes.Yellow, new PropertyChangedCallback(SelectText)));


        public static Brush GetForecolor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(ForecolorProperty);
        }

        public static void SetForecolor(DependencyObject obj, Brush value)
        {
            obj.SetValue(ForecolorProperty, value);
        }

        public static readonly DependencyProperty ForecolorProperty =
            DependencyProperty.RegisterAttached("Forecolor", typeof(Brush), typeof(TextBlockHighlighterAuthors),
                new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(SelectText)));

        public static bool GetWordBreakMode(DependencyObject obj)
        {
            return (bool)obj.GetValue(WordBreakModeProperty);
        }

        public static void SetWordBreakMode(DependencyObject obj, bool value)
        {
            obj.SetValue(WordBreakModeProperty, value);
        }

        public static readonly DependencyProperty WordBreakModeProperty =
            DependencyProperty.RegisterAttached("WordBreakMode", typeof(bool), typeof(TextBlockHighlighterAuthors),
                new PropertyMetadata(false, new PropertyChangedCallback(SelectText)));
    }
}
