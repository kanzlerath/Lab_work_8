using System;
using System.Text.RegularExpressions;

namespace Lab1_compile
{
    public class RegexPatterns
    {
        public static readonly string FilePattern = @"[a-zA-Z0-9_\-А-яЁё]+\.(doc|docx|pdf)";

        public static readonly string MaestroCardPattern = @"(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}";

        public static readonly string IPv6Pattern = @"([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}"; 

        public static MatchCollection FindMatches(string text, string pattern)
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.Matches(text);
        }
    }
} 