using System;
using System.Threading.Tasks;
using TgTranslator.Interfaces;

namespace TgTranslator.Services.Translation;

public class TranslatePlaceholderService : ITranslator, ILanguageDetector
{
    private const string ImplementMessage = "You have to implement the translation service for yourself!\nUse ITranslator and ILanguageDetector and add your service to DiServices.cs";
    public TranslatePlaceholderService()
    {
        throw new NotImplementedException(ImplementMessage);
    }

    public Task<string> TranslateTextAsync(string text, string to)
    {
        throw new NotImplementedException(ImplementMessage);
    }

    public Task<string> DetectLanguageAsync(string text)
    {
        throw new NotImplementedException(ImplementMessage);
    }
}