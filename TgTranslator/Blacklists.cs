using System.Collections.Generic;
using System.Linq;

namespace TgTranslator
{
    static class Blacklists
    {
        public static readonly List<string> LanguagesBlacklist = new List<string> { "en", "" };
        //public static List<string> wordsBlacklist = new List<string> { };

        public static bool Verify(string text, IEnumerable<string> blacklist)
        {
            return blacklist.All(x => x != text);
        }
    }
}
