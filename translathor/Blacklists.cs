using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace translathor
{
    class Blacklists
    {
        public static List<string> languagesBlacklist = new List<string> { "en", "" };
        public static List<string> wordsBlacklist = new List<string> { };

        public static bool Verify(string text, List<string> blacklist)
        {
            // Тут можно заюзать Linq, но мне лень 
            foreach (var word in blacklist)
            {
                if (text.Contains(word))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEnglish(string input)
        {
            Regex regex = new Regex(@"[A-Za-z0-9 .,-=@+(){}\[\]\\]");
            MatchCollection matches = regex.Matches(input);

            if (matches.Count.Equals(input.Length))
                return true;
            else
                return false;
        }
    }
}
