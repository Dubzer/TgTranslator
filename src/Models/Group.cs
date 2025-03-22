using System.Collections.Generic;
using TgTranslator.Menu;

namespace TgTranslator.Models;

public partial class Group
{
    public long GroupId { get; set; }
    public ICollection<string> Languages { get; set; }
    public TranslationMode TranslationMode { get; set; }
    public int Delay { get; set; }
    public bool TranslateWithLinks { get; set; }

    public virtual GroupBlacklist GroupBlacklist { get; set; }
}