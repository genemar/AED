using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace AEDFinder.Web.Models;

public class Aed
{
    public int Id { get; set; }

    [Required]
    public string OwnerId { get; set; } = default!;

    public IdentityUser? Owner { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string HouseNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string City { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LocationDetail { get; set; }

    [Phone]
    [MaxLength(30)]
    public string? ContactNumber { get; set; }

    [MaxLength(100)]
    public string? DeviceType { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [MaxLength(1000)]
    public string? AccessInstructions { get; set; }

    [MaxLength(1000)]
    public string? LastMinuteInstructions { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<AedAvailabilityWindow> AvailabilityWindows { get; set; } = new List<AedAvailabilityWindow>();

    public bool IsAvailableAt(DateTimeOffset moment)
    {
        if (!IsActive)
        {
            return false;
        }

        if (AvailabilityWindows.Count == 0)
        {
            return true;
        }

        var local = moment.LocalDateTime;
        var day = local.DayOfWeek;
        var time = local.TimeOfDay;

        foreach (var window in AvailabilityWindows.Where(w => w.DayOfWeek == day))
        {
            if (window.Covers(time))
            {
                return true;
            }
        }

        return false;
    }
}
