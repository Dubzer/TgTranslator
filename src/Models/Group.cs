using TgTranslator.Menu;

namespace TgTranslator.Models
{
    public class Group
    {
        public long GroupId { get; set; }
        public string Language { get; set; } = "en";
        public TranslationMode TranslationMode { get; set; } = TranslationMode.Auto;
    }
}
