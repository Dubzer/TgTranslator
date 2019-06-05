using System.Collections.Generic;
using System.Linq;

namespace Translathor
{
    class Blacklists
    {
        public static List<string> languagesBlacklist = new List<string> { "en", "" };
        //public static List<string> wordsBlacklist = new List<string> { };

        public static bool Verify(string text, List<string> blacklist)
        {
            if (blacklist.Where(x => x == text).Any())
            {
                return false;
            }

            return true;
        }
    }
}