namespace AEDFinder.Web.Models;

public class AedSearchResult
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string HouseNumber { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? ContactNumber { get; set; }

    public string? AccessInstructions { get; set; }

    public string? LastMinuteInstructions { get; set; }

    public double? DistanceInMeters { get; set; }

    public string GetFormattedDistance()
    {
        if (!DistanceInMeters.HasValue)
        {
            return "";
        }

        return DistanceInMeters.Value >= 1000
            ? $"{DistanceInMeters.Value / 1000:0.0} km"
            : $"{DistanceInMeters.Value:0} m";
    }
}
