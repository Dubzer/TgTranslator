using System.Collections.Immutable;

namespace TgTranslator
{
    public class Blacklist
    {
        private readonly ImmutableHashSet<int> _usersBlacklist;
        private readonly ImmutableHashSet<long> _groupsBlacklist;
        private readonly ImmutableHashSet<string> _textsBlacklist;

        public Blacklist(ImmutableHashSet<long> groupsBlacklist, ImmutableHashSet<int> usersBlacklist, ImmutableHashSet<string> textsBlacklist)
        {
            _groupsBlacklist = groupsBlacklist;
            _usersBlacklist = usersBlacklist;
            _textsBlacklist = textsBlacklist;
        }

        public bool IsGroupBlocked(long id)
        {
            return _groupsBlacklist.Contains(id);
        }
        
        public bool IsUserBlocked(int id)
        {
            return _usersBlacklist.Contains(id);
        }

        public bool IsTextAllowed(string text)
        {
            return !_textsBlacklist.Contains(text);
        }
    }
}