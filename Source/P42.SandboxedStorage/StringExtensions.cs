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

        public static string RemoveCurrentFolderFromPath(this IStorageFolder folder, string path)
        {
            var folderPath = folder.Path;
            if (folderPath[folderPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                folderPath += System.IO.Path.DirectorySeparatorChar;

            for (int i = 0; i < folderPath.Length && i < path.Length; i++)
            {
                if (folderPath[i] != path[i])
                    return null;
            }
            if (folderPath.Length < path.Length)
            {
                var fileNAme = path.Substring(folderPath.Length);
                return fileNAme;

            }
            return null;
        }

    }
}
