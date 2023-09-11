using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TgTranslator.Utils.Extensions;

public static class AspNetCodeExtensions
{
    // https://markb.uk/asp-net-core-read-raw-request-body-as-string.html
    public static async Task<string> GetRawBodyAsync(
        this HttpRequest request,
        Encoding encoding = null)
    {
        if (!request.Body.CanSeek)
            request.EnableBuffering();

        request.Body.Position = 0;

        var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }
}