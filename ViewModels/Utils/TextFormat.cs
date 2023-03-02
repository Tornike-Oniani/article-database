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
        private static readonly Regex unusualCharacters = new Regex("[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+");

        public static string RemoveSpareWhiteSpace(string input)
        {
            return unusualWhiteSpace.Replace(input, " ").Trim();
        }
        public static string[] GetUnusualCharacters(string input)
        {
            return unusualCharacters.Matches(input).Cast<Match>().Select(match => match.Value).ToArray();
        }
    }
}
