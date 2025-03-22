#nullable enable
using System.Collections.Frozen;
using System.Linq;
using Microsoft.Extensions.Options;
using TgTranslator.Data.Options;

namespace TgTranslator.Services;

public class LanguagesProvider
{
    private readonly LanguagesList _list;
    private FrozenDictionary<string, string>? _codesToNames;

    public LanguagesProvider(IOptions<LanguagesList> list)
    {
        _list = list.Value;
    }

    // returning FrozenDictionary to avoid nullability issues
    private FrozenDictionary<string, string> Initialize()
    {
        _codesToNames = _list.ToDictionary(x => x.Code, x => x.Name).ToFrozenDictionary();
        return _codesToNames;
    }

    public string GetName(string code)
    {
        var dict = _codesToNames ?? Initialize();
        return dict[code];
    }
}