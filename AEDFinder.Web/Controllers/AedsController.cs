using AEDFinder.Web.Models;
using AEDFinder.Web.Models.ViewModels;
using AEDFinder.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AEDFinder.Web.Controllers;

[Authorize]
public class AedsController(IAedService aedService, UserManager<IdentityUser> userManager, IOptions<MapOptions> mapOptions) : Controller
{
    private readonly IAedService _aedService = aedService;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly MapOptions _mapOptions = mapOptions.Value;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        var items = await _aedService.GetForOwnerAsync(ownerId, cancellationToken);
        return View(items);
    }

    public IActionResult Create()
    {
        var model = new AedFormViewModel();
        ConfigureMap(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AedFormViewModel model, CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            ConfigureMap(model);
            return View(model);
        }

        var (entity, windows) = model.ToEntity(ownerId);
        await _aedService.CreateAsync(entity, windows, cancellationToken);

        TempData["StatusMessage"] = "AED is opgeslagen.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        var entity = await _aedService.GetForOwnerAsync(id, ownerId, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var viewModel = AedFormViewModel.FromEntity(entity);
        ConfigureMap(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AedFormViewModel model, CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ConfigureMap(model);
            return View(model);
        }

        var (entity, windows) = model.ToEntity(ownerId);
        entity.Id = id;

        try
        {
            await _aedService.UpdateAsync(entity, windows, cancellationToken);
            TempData["StatusMessage"] = "Wijzigingen opgeslagen.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        var entity = await _aedService.GetForOwnerAsync(id, ownerId, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var ownerId = _userManager.GetUserId(User);
        if (ownerId is null)
        {
            return Challenge();
        }

        try
        {
            await _aedService.DeleteAsync(id, ownerId, cancellationToken);
            TempData["StatusMessage"] = "AED verwijderd.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    private void ConfigureMap(AedFormViewModel model)
    {
        ViewBag.MapTileUrl = _mapOptions.TileUrl;
        ViewBag.MapAttribution = _mapOptions.Attribution;
        ViewBag.MapDefaultLat = _mapOptions.DefaultLatitude;
        ViewBag.MapDefaultLng = _mapOptions.DefaultLongitude;
        ViewBag.MapDefaultZoom = _mapOptions.DefaultZoom;
        ViewBag.MapSelectedLat = model.Latitude;
        ViewBag.MapSelectedLng = model.Longitude;
    }
}
