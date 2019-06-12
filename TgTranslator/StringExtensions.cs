using System.Text.RegularExpressions;

namespace Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes links from string
        /// </summary>
        /// <returns>string without links</returns>
        public static string WithoutLinks(this string inputString)
        {
            if (Regex.Matches(inputString, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?").Count != 0)
            {
                return Regex.Replace(inputString, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", "");
            }

            return inputString;
        }

        public static string WithoutArguments(this string inputString)
        {
            return inputString.Split(' ')[0];
        }
    }
}


