using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace TgTranslator.Services;

public class TelegramApiLoggingStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        builder =>
        {
            var client = builder.ApplicationServices.GetRequiredService<TelegramBotClient>();
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<TelegramApiLoggingStartupFilter>>();
            client.OnApiResponseReceived += async (_, args, cancellationToken) =>
            {
                if (args.ResponseMessage.StatusCode != HttpStatusCode.TooManyRequests)
                    return;

                int? retryAfterSeconds = null;
                var responseBody = await args.ResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    using var response = JsonDocument.Parse(responseBody);
                    if (response.RootElement.TryGetProperty("parameters", out var parameters)
                        && parameters.TryGetProperty("retry_after", out var retryAfter))
                        retryAfterSeconds = retryAfter.GetInt32();
                }
                catch (JsonException)
                {
                    // An intermediary may return a non-JSON 429 response.
                }

                logger.LogWarning("Telegram API rate limited {TelegramMethod}; retry after {RetryAfterSeconds}s",
                    args.ApiRequestEventArgs.Request.MethodName,
                    retryAfterSeconds);
            };

            next(builder);
        };
}
