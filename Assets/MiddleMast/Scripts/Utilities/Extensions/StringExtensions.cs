using System;
using System.Text.RegularExpressions;

namespace MiddleMast.Utilities.Extensions
{
    public static class StringExtensions
    {
        static Regex DoubleSpaceRegex { get
            {
                if (doubleSpaceRegex == null) 
                {
                    RegexOptions __options = RegexOptions.None;
                    doubleSpaceRegex = new Regex("[ ]{2,}", __options);
                }
                return doubleSpaceRegex;
            } 
        }
        static Regex doubleSpaceRegex;

        public static int CountLines(this string s)
        {
            return s.Split('\n').Length;
        }

        /// <summary>
        /// Removes duplicate spaces from a string. PROBABLY GENERATES ALLOCATIONS
        /// </summary>
        public static string RemoveDoubleSpaces(this string s)
        {
            return DoubleSpaceRegex.Replace(s, " ");
        }
        
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}