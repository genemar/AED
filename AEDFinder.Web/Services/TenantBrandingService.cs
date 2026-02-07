using System;
using System.Linq;
using AEDFinder.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AEDFinder.Web.Services;

public interface ITenantBrandingService
{
    TenantTheme Resolve(HttpContext context);
}

public class TenantBrandingService(IOptions<TenantBrandingOptions> options) : ITenantBrandingService
{
    private readonly TenantBrandingOptions _options = options.Value;

    public TenantTheme Resolve(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var theme = _options.Tenants
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Host) && string.Equals(t.Host, host, StringComparison.OrdinalIgnoreCase));

        return theme ?? _options.Default;
    }
}
