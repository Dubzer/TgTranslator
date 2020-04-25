using System.Collections.Generic;

namespace TgTranslator.Data.Options
{
    public class Blacklists
    {
        public HashSet<long> GroupIdsBlacklist { get; set; }
        public HashSet<int> UserIdsBlacklist { get; set; }
        public HashSet<string> TextsBlacklist { get; set; }
    }
}