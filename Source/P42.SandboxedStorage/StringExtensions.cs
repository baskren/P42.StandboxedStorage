using System;
using System.Text.RegularExpressions;

namespace P42.SandboxedStorage
{
    static class StringExtensions
    {
        public static string WildcardToRegex(this string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }
    }
}
