using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace TgTranslator.Services.Middlewares;

internal sealed class BasicAuthMiddleware
{
    private const string HeadersField = "Authorization";

    private const byte FailedAuthorizationsCooldownInHours = 1;
    private const byte MaxFailedAuthorizationAttempts = 5;

    private static readonly SemaphoreSlim AuthorizationSemaphore = new SemaphoreSlim(1, 1);

    private static readonly ConcurrentDictionary<IPAddress, byte> FailedAuthorizations =
        new ConcurrentDictionary<IPAddress, byte>();

    private static Timer _clearFailedAuthorizationsTimer;

    private readonly string _login;
    private readonly RequestDelegate _next;
    private readonly string _password;

    public BasicAuthMiddleware(RequestDelegate next, string login, string password)
    {
        _login = login;
        _password = password;

        _next = next ?? throw new ArgumentNullException(nameof(next));

        lock (FailedAuthorizations)
        {
            if (_clearFailedAuthorizationsTimer == null)
                _clearFailedAuthorizationsTimer = new Timer(e => FailedAuthorizations.Clear(),
                    null,
                    TimeSpan.FromHours(FailedAuthorizationsCooldownInHours), // Delay
                    TimeSpan.FromHours(FailedAuthorizationsCooldownInHours) // Period
                );
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task InvokeAsync(HttpContext context)
    {
        HttpStatusCode authenticationStatus = await GetAuthenticationStatus(context).ConfigureAwait(false);

        if (authenticationStatus != HttpStatusCode.OK)
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;

            await context.Response.WriteAsync($@"{(ushort) authenticationStatus} - {authenticationStatus}")
                .ConfigureAwait(false);

            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private async Task<HttpStatusCode> GetAuthenticationStatus(HttpContext context)
    {
        if (context == null)
            return HttpStatusCode.InternalServerError;

        if (string.IsNullOrEmpty(_password))
            return HttpStatusCode.OK;

        IPAddress clientIp = context.Connection.RemoteIpAddress;

        if (FailedAuthorizations.TryGetValue(clientIp, out byte attempts))
            if (attempts >= MaxFailedAuthorizationAttempts)
                return HttpStatusCode.Forbidden;

        (string login, string password) = GetCredentialsFromHeaders(context.Request.Headers);

        if (login == null || password == null)
            return HttpStatusCode.Unauthorized;

        bool authorized = login == _login && password == _password;

        await AuthorizationSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (FailedAuthorizations.TryGetValue(clientIp, out attempts))
                if (attempts >= MaxFailedAuthorizationAttempts)
                    return HttpStatusCode.Forbidden;

            if (!authorized)
                FailedAuthorizations[clientIp] = FailedAuthorizations.TryGetValue(clientIp, out attempts)
                    ? ++attempts
                    : (byte) 1;
        }
        finally
        {
            AuthorizationSemaphore.Release();
        }

        return authorized ? HttpStatusCode.OK : HttpStatusCode.Unauthorized;
    }

    private (string, string) GetCredentialsFromHeaders(IHeaderDictionary headers)
    {
        string[] credentials = headers.TryGetValue(HeadersField, out StringValues authHeader)
            ? Encoding.Default.GetString(Convert.FromBase64String(authHeader.First().Replace("Basic ", "")))
                .Split(":")
            : null;

        return credentials != null
            ? (credentials[0], credentials[1])
            : (null, null);
    }
}