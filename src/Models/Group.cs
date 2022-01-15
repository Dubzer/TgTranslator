using TgTranslator.Menu;

namespace TgTranslator.Models;

public partial class Group
{
    public long GroupId { get; set; }
    public string Language { get; set; }
    public TranslationMode TranslationMode { get; set; }

    public virtual GroupBlacklist GroupBlacklist { get; set; }
}