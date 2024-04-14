using System.ComponentModel.DataAnnotations;
using TgTranslator.Menu;

namespace TgTranslator.Data.DTO;

public record Settings
{
    public TranslationMode TranslationMode { get; init; }

    public string[] Languages { get; init; }

    [Range(0, 5)]
    public int Delay { get; init; }
}