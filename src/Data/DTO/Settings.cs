using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Options;
using TgTranslator.Data.Options;
using TgTranslator.Menu;

namespace TgTranslator.Data.DTO;

public record Settings
{
    public TranslationMode TranslationMode { get; init; }

    public string[] Languages { get; init; }

    public int Delay { get; init; }
}

public class SettingsValidator : AbstractValidator<Settings>
{
    private readonly FrozenSet<string> _languageCodes;

    public SettingsValidator(IOptions<LanguagesList> languages)
    {
        _languageCodes = new HashSet<string>(languages.Value.Select(x => x.Code)).ToFrozenSet();

        RuleForEach(x => x.Languages).Must(ValidateLanguage);
        RuleFor(x => x.Delay).Must(ValidateDelay);
    }

    public static bool ValidateMode(string mode) =>
        Enum.TryParse(typeof(TranslationMode), mode, true, out _);

    public bool ValidateLanguage(string language) =>
        _languageCodes.Contains(language);

    public static bool ValidateDelay(int delay) => delay is >= 0 and < 6;
}