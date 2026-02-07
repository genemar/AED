using System;

namespace AEDFinder.Web.Models.ViewModels;

public class HomeViewModel
{
    public TenantTheme Theme { get; init; } = new();

    public MapOptions Map { get; init; } = new();

    public IReadOnlyList<HomeAedListItem> Featured { get; init; } = Array.Empty<HomeAedListItem>();
}

public class HomeAedListItem
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string? ContactNumber { get; init; }

    public string? AccessInstructions { get; init; }

    public string? LastMinuteInstructions { get; init; }

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }
}
