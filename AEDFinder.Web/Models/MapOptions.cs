namespace AEDFinder.Web.Models;

public class MapOptions
{
    public string TileUrl { get; set; } = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

    public string Attribution { get; set; } = "&copy; OpenStreetMap contributors";

    public double DefaultLatitude { get; set; } = 52.1326;

    public double DefaultLongitude { get; set; } = 5.2913;

    public int DefaultZoom { get; set; } = 7;
}
