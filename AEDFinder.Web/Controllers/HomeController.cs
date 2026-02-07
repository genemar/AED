using System.Diagnostics;
using System.Linq;
using AEDFinder.Web.Models;
using AEDFinder.Web.Models.ViewModels;
using AEDFinder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AEDFinder.Web.Controllers;

public class HomeController(IAedService aedService, ITenantBrandingService brandingService, IOptions<MapOptions> mapOptions) : Controller
{
    private readonly IAedService _aedService = aedService;
    private readonly ITenantBrandingService _brandingService = brandingService;
    private readonly MapOptions _mapOptions = mapOptions.Value;

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Finder));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Finder(CancellationToken cancellationToken)
    {
        var theme = _brandingService.Resolve(HttpContext);
        var available = await _aedService.GetAvailableAsync(DateTimeOffset.UtcNow, cancellationToken);

        var featured = available
            .Take(5)
            .Select(a => new HomeAedListItem
            {
                Id = a.Id,
                Title = string.IsNullOrWhiteSpace(a.DisplayName) ? $"AED bij {a.Street} {a.HouseNumber}" : a.DisplayName,
                Address = $"{a.Street} {a.HouseNumber}, {a.PostalCode} {a.City}",
                ContactNumber = a.ContactNumber,
                AccessInstructions = a.AccessInstructions,
                LastMinuteInstructions = a.LastMinuteInstructions,
                Latitude = a.Latitude,
                Longitude = a.Longitude
            })
            .ToList();

        var model = new HomeViewModel
        {
            Theme = theme,
            Map = _mapOptions,
            Featured = featured
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
