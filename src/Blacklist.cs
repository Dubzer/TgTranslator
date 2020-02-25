using System.Collections.Immutable;

namespace TgTranslator
{
    public class Blacklist
    {
        private readonly ImmutableHashSet<long> _groupsBlacklist;
        private readonly ImmutableHashSet<string> _textsBlacklist;
        private readonly ImmutableHashSet<int> _usersBlacklist;

        public Blacklist(ImmutableHashSet<long> groupsBlacklist, ImmutableHashSet<int> usersBlacklist, ImmutableHashSet<string> textsBlacklist)
        {
            _groupsBlacklist = groupsBlacklist;
            _usersBlacklist = usersBlacklist;
            _textsBlacklist = textsBlacklist;
        }

        public bool IsGroupBlocked(long id) => _groupsBlacklist.Contains(id);

        public bool IsUserBlocked(int id) => _usersBlacklist.Contains(id);

        public bool IsTextAllowed(string text) => !_textsBlacklist.Contains(text);
    }
}