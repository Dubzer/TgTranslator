using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TgTranslator.Data.Options;

namespace TgTranslator.Services;

public class WebAppHashService
{
    private readonly byte[] _token;

    public WebAppHashService(IOptions<TelegramOptions> options)
    {
        _token = HMACSHA256.HashData("WebAppData"u8, Encoding.UTF8.GetBytes(options.Value.BotToken));
    }
    
    public bool VerifyHash(string queryString, string hash)
    {
        var generatedHash = HMACSHA256.HashData(
            _token,
            Encoding.UTF8.GetBytes(queryString));

        return Convert.FromHexString(hash).SequenceEqual(generatedHash);
    }
}