using AEDFinder.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AEDFinder.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AedsController(IAedService aedService) : ControllerBase
{
    private readonly IAedService _aedService = aedService;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] double? lat, [FromQuery] double? lng, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var items = await _aedService.SearchAvailableAsync(now, lat, lng, cancellationToken);

        var response = items.Select(item => new
        {
            item.Id,
            item.DisplayName,
            item.Street,
            item.HouseNumber,
            item.PostalCode,
            item.City,
            item.Latitude,
            item.Longitude,
            item.ContactNumber,
            item.AccessInstructions,
            item.LastMinuteInstructions,
            DistanceMeters = item.DistanceInMeters,
            DistanceText = item.GetFormattedDistance()
        });

        return Ok(response);
    }
}
