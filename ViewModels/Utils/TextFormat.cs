using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public static class TextFormat
    {
        private static string allowedCharacters = Properties.Settings.Default.AllowedCharacters;
        private static readonly Regex unusualWhiteSpace = new Regex("\\s{2,}|[\\t\\n\\r]");
        private static readonly Regex unprintableCharacters = new Regex("[\x00-\x1f]+");
        private static Regex unusualCharacters = new Regex($@"[^\x20-\x7e{allowedCharacters}]+");
        private static Regex isValidText = new Regex($@"^[\x20-\x7e{allowedCharacters}]+$");

        public static string RemoveSpareWhiteSpace(string input)
        {
            return unusualWhiteSpace.Replace(input, " ");
        }
        public static string RemoveUnprintableCharacters(string input)
        {
            return unprintableCharacters.Replace(input, "");
        }
        public static string RemoveLineBreaks(string input)
        {
            return input.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
        }
        public static string[] GetUnusualCharacters(string input)
        {
            if (allowedCharacters != Properties.Settings.Default.AllowedCharacters)
            {
                allowedCharacters = Properties.Settings.Default.AllowedCharacters;
                unusualCharacters = new Regex($@"[^\x20-\x7e{allowedCharacters}]+");
                isValidText = new Regex($@"^[\x20-\x7e{allowedCharacters}]+$");
            }
            return unusualCharacters.Matches(input).Cast<Match>().Select(match => match.Value).Distinct().ToArray();
        }
        public static bool IsValidText(string input)
        {
            if (allowedCharacters != Properties.Settings.Default.AllowedCharacters)
            {
                allowedCharacters = Properties.Settings.Default.AllowedCharacters;
                unusualCharacters = new Regex($@"[^\x20-\x7e{allowedCharacters}]+");
                isValidText = new Regex($@"^[\x20-\x7e{allowedCharacters}]+$");
            }

            return isValidText.IsMatch(input);
        }
    }
}
