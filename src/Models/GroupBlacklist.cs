using System;

namespace TgTranslator.Models
{
    public partial class GroupBlacklist
    {
        public long GroupId { get; set; }
        public DateTime AddedAt { get; set; }

        public virtual Group Group { get; set; }
    }
}
