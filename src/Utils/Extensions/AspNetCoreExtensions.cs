using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TgTranslator.Utils.Extensions;

public static class AspNetCoreExtensions
{
    public static IHostApplicationBuilder ConfigureOptionFromSection<T>(this IHostApplicationBuilder builder, string key)
        where T : class
    {
        builder.Services.Configure<T>(builder.Configuration.GetSection(key));
        return builder;
    }

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