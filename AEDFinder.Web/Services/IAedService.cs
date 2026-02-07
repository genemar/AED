using AEDFinder.Web.Models;

namespace AEDFinder.Web.Services;

public interface IAedService
{
    Task<IReadOnlyList<Aed>> GetAvailableAsync(DateTimeOffset timestamp, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AedSearchResult>> SearchAvailableAsync(DateTimeOffset timestamp, double? latitude, double? longitude, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aed>> GetForOwnerAsync(string ownerId, CancellationToken cancellationToken = default);

    Task<Aed?> GetForOwnerAsync(int id, string ownerId, CancellationToken cancellationToken = default);

    Task<Aed> CreateAsync(Aed aed, IEnumerable<AedAvailabilityWindow> availabilityWindows, CancellationToken cancellationToken = default);

    Task<Aed> UpdateAsync(Aed aed, IEnumerable<AedAvailabilityWindow> availabilityWindows, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string ownerId, CancellationToken cancellationToken = default);
}
