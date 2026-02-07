using System.Linq;
using AEDFinder.Web.Data;
using AEDFinder.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AEDFinder.Web.Services;

public class AedService(ApplicationDbContext context, ILogger<AedService> logger) : IAedService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<AedService> _logger = logger;

    public async Task<IReadOnlyList<Aed>> GetAvailableAsync(DateTimeOffset timestamp, CancellationToken cancellationToken = default)
    {
        var items = await _context.Aeds
            .AsNoTracking()
            .Include(a => a.AvailabilityWindows)
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        var available = items.Where(a => a.IsAvailableAt(timestamp)).ToList();
        _logger.LogDebug("{Count} AED(s) beschikbaar op {Timestamp}", available.Count, timestamp);
        return available;
    }

    public async Task<IReadOnlyList<AedSearchResult>> SearchAvailableAsync(DateTimeOffset timestamp, double? latitude, double? longitude, CancellationToken cancellationToken = default)
    {
        var available = await GetAvailableAsync(timestamp, cancellationToken);
        _logger.LogDebug("Start zoeken naar AEDs rond ({Latitude}, {Longitude})", latitude, longitude);

        var results = available
            .Where(a => a.Latitude.HasValue && a.Longitude.HasValue)
            .Select(a => new AedSearchResult
            {
                Id = a.Id,
                DisplayName = string.IsNullOrWhiteSpace(a.DisplayName) ? $"AED bij {a.Street} {a.HouseNumber}" : a.DisplayName,
                Street = a.Street,
                HouseNumber = a.HouseNumber,
                PostalCode = a.PostalCode,
                City = a.City,
                Latitude = a.Latitude!.Value,
                Longitude = a.Longitude!.Value,
                AccessInstructions = a.AccessInstructions,
                LastMinuteInstructions = a.LastMinuteInstructions,
                ContactNumber = a.ContactNumber,
                DistanceInMeters = CalculateDistanceMeters(latitude, longitude, a.Latitude, a.Longitude)
            })
            .OrderBy(r => r.DistanceInMeters ?? double.MaxValue)
            .ToList();

        return results;
    }

    public async Task<IReadOnlyList<Aed>> GetForOwnerAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Aeds
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .Include(a => a.AvailabilityWindows)
            .OrderBy(a => a.DisplayName)
            .ThenBy(a => a.Street)
            .ToListAsync(cancellationToken);
    }

    public async Task<Aed?> GetForOwnerAsync(int id, string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Aeds
            .AsNoTracking()
            .Include(a => a.AvailabilityWindows)
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId, cancellationToken);
    }

    public async Task<Aed> CreateAsync(Aed aed, IEnumerable<AedAvailabilityWindow> availabilityWindows, CancellationToken cancellationToken = default)
    {
        var windows = availabilityWindows.ToList();
        foreach (var window in windows)
        {
            aed.AvailabilityWindows.Add(new AedAvailabilityWindow
            {
                DayOfWeek = window.DayOfWeek,
                StartTime = window.StartTime,
                EndTime = window.EndTime
            });
        }
        aed.CreatedAt = DateTimeOffset.UtcNow;
        aed.UpdatedAt = null;

        _context.Aeds.Add(aed);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Nieuwe AED geregistreerd met id {Id}", aed.Id);
        return aed;
    }

    public async Task<Aed> UpdateAsync(Aed updated, IEnumerable<AedAvailabilityWindow> availabilityWindows, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Aeds
            .Include(a => a.AvailabilityWindows)
            .FirstOrDefaultAsync(a => a.Id == updated.Id && a.OwnerId == updated.OwnerId, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException("AED niet gevonden of geen toegang.");
        }

        existing.DisplayName = updated.DisplayName;
        existing.Street = updated.Street;
        existing.HouseNumber = updated.HouseNumber;
        existing.PostalCode = updated.PostalCode;
        existing.City = updated.City;
        existing.LocationDetail = updated.LocationDetail;
        existing.ContactNumber = updated.ContactNumber;
        existing.DeviceType = updated.DeviceType;
        existing.ExpirationDate = updated.ExpirationDate;
        existing.AccessInstructions = updated.AccessInstructions;
        existing.LastMinuteInstructions = updated.LastMinuteInstructions;
        existing.Latitude = updated.Latitude;
        existing.Longitude = updated.Longitude;
        existing.IsActive = updated.IsActive;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        _context.AedAvailabilityWindows.RemoveRange(existing.AvailabilityWindows);
        existing.AvailabilityWindows.Clear();
        foreach (var window in availabilityWindows)
        {
            existing.AvailabilityWindows.Add(new AedAvailabilityWindow
            {
                DayOfWeek = window.DayOfWeek,
                StartTime = window.StartTime,
                EndTime = window.EndTime
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AED {Id} bijgewerkt", existing.Id);

        return existing;
    }

    public async Task DeleteAsync(int id, string ownerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Aeds.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId, cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException("AED niet gevonden of geen toegang.");
        }

        _context.Aeds.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AED {Id} verwijderd", entity.Id);
    }

    private static double? CalculateDistanceMeters(double? userLat, double? userLng, double? aedLat, double? aedLng)
    {
        if (!userLat.HasValue || !userLng.HasValue || !aedLat.HasValue || !aedLng.HasValue)
        {
            return null;
        }

        const double earthRadius = 6371000;
        var dLat = ToRadians(aedLat.Value - userLat.Value);
        var dLon = ToRadians(aedLng.Value - userLng.Value);

        var lat1 = ToRadians(userLat.Value);
        var lat2 = ToRadians(aedLat.Value);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
}
