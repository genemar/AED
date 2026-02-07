using AEDFinder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AEDFinder.Web.Filters;

public class TenantBrandingFilter(ITenantBrandingService brandingService) : IAsyncActionFilter
{
    private readonly ITenantBrandingService _brandingService = brandingService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            var theme = _brandingService.Resolve(controller.HttpContext);
            controller.ViewBag.BrandName = theme.Name;
            controller.ViewBag.ThemePrimaryColor = theme.PrimaryColor;
            controller.ViewBag.ThemeAccentColor = theme.AccentColor;
            controller.ViewBag.ThemeLogoPath = theme.LogoPath;
        }

        await next();
    }
}
