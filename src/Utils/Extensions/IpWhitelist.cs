using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using NetTools;
using Serilog;
using TgTranslator.Data.Options;

namespace TgTranslator.Utils.Extensions;

public class IpWhitelist : ActionFilterAttribute
{
    private readonly TelegramOptions _config;
    private readonly IEnumerable<IPAddressRange> _allowedIps;

    public IpWhitelist(IOptions<TelegramOptions> config)
    {
        _config = config.Value;
        _allowedIps = config.Value.TelegramIpWhitelist.Select(IPAddressRange.Parse);
    }
        
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IPAddress? clientIp = string.IsNullOrEmpty(_config.CustomIpHeader)
            ? context.HttpContext.Connection.RemoteIpAddress
            : IPAddress.Parse(context.HttpContext.Request.Headers[_config.CustomIpHeader]);
            
        if (!_allowedIps.Any(range => range.Contains(clientIp)))
        {
            context.Result = new StatusCodeResult((int) HttpStatusCode.Forbidden);
            Log.Warning("Got a request that is not contained in TelegramIpWhitelist: {IpAddress}", clientIp);
        }
    }
}