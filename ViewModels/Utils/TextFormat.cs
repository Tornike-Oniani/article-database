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
        private static readonly Regex unusualWhiteSpace = new Regex("\\s{2,}|[\\t\\n\\r]");
        private static readonly Regex unprintableCharacters = new Regex("[^\x20-\x7E]+");
        private static readonly Regex unusualCharacters = new Regex("[^A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+");
        private static readonly Regex isValidText = new Regex("^[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+$");

        public static string RemoveSpareWhiteSpace(string input)
        {
            return unusualWhiteSpace.Replace(input, " ");
        }
        public static string RemoveUnprintableCharacters(string input)
        {
            return unprintableCharacters.Replace(input, "");
        }
        public static string[] GetUnusualCharacters(string input)
        {
            return unusualCharacters.Matches(input).Cast<Match>().Select(match => match.Value).Distinct().ToArray();
        }
        public static bool IsValidText(string input)
        {
            return isValidText.IsMatch(input);
        }
    }
}
