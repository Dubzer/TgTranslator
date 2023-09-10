using TgTranslator.Menu;

namespace TgTranslator.Data.DTO;

public class Settings
{
    public TranslationMode TranslationMode { get; set; }

    public string[] Languages { get; set; }
}