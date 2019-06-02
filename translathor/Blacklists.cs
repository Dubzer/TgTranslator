using System.Collections.Generic;

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
    }
}
