using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Lib.Views.Attached_Properties
{
    public class RegexTextBlockHighlighter
    {
        // Word highlights
        public static List<string> GetWordSelection(DependencyObject obj)
        {
            return (List<string>)obj.GetValue(WordSelectionProperty);
        }
        public static void SetWordSelection(DependencyObject obj, List<string> value)
        {
            obj.SetValue(WordSelectionProperty, value);
        }
        public static readonly DependencyProperty WordSelectionProperty =
            DependencyProperty.RegisterAttached("WordSelection", typeof(List<string>), typeof(RegexTextBlockHighlighter),
                new PropertyMetadata(new PropertyChangedCallback(SelectText)));

        // Phrase highlights
        public static List<string> GetPhraseSelection(DependencyObject obj)
        {
            return (List<string>)obj.GetValue(PhraseSelectionProperty);
        }
        public static void SetPhraseSelection(DependencyObject obj, List<string> value)
        {
            obj.SetValue(PhraseSelectionProperty, value);
        }
        public static readonly DependencyProperty PhraseSelectionProperty =
        DependencyProperty.RegisterAttached("PhraseSelection", typeof(List<string>), typeof(RegexTextBlockHighlighter),
            new PropertyMetadata(new PropertyChangedCallback(SelectText)));

        private static void SelectText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Select(d);
        }

        private static void Select(DependencyObject d)
        {
            if (d == null) return;
            if (!(d is TextBlock)) throw new InvalidOperationException("Only valid for TextBlock");

            TextBlock txtBlock = d as TextBlock;
            string text = txtBlock.Text;
            if (string.IsNullOrEmpty(text)) return;

            string[] highlightWords = GetWordSelection(d) == null ? new string[] { } : GetWordSelection(d).ToArray();
            string[] highlightPhrases = GetPhraseSelection(d) == null ? new string[] { } : GetPhraseSelection(d).ToArray();
            // If we don't have either words or phrases to highlight don't do anything
            if ((highlightWords.Length == 0 && highlightPhrases.Length == 0)) { return; }

            Brush selectionColor = (Brush)d.GetValue(HighlightColorProperty);
            Brush forecolor = (Brush)d.GetValue(ForecolorProperty);

            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();

            int wordIndex = 1;
            Regex regex;
            foreach (string highlightText in highlightWords)
            {
                //wordIndex = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);                
                string escapeText = Regex.Escape(highlightText);
                regex = new Regex($@"(?i)(?<!\w)(?:\w?{escapeText}|{escapeText}\w?)(?!\w)");
                if (wordIndex != -1)
                {
                    //indexes.Add(new Tuple<int, int>(wordIndex, highlightText.Length));
                    indexes.Add(new Tuple<int, int>(regex.Match(text).Index, regex.Match(text).Value.Length));
                }
            }
            foreach (string highlightText in highlightPhrases)
            {
                for (int index = 0; ; index += highlightText.Length)
                {
                    index = text.IndexOf(highlightText, index, StringComparison.CurrentCultureIgnoreCase);
                    if (index == -1)
                        break;
                    indexes.Add(new Tuple<int, int>(index, highlightText.Length));
                }
            }

            if (indexes.Count == 0) { return; }

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
                        // newIndexes.Add(currentItem);
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
            DependencyProperty.RegisterAttached("HighlightColor", typeof(Brush), typeof(RegexTextBlockHighlighter),
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
            DependencyProperty.RegisterAttached("Forecolor", typeof(Brush), typeof(RegexTextBlockHighlighter),
                new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(SelectText)));
    }
}
