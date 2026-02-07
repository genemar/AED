using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AEDFinder.Web.Models.ViewModels;

public class AedFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Naam of omschrijving")]
    public string? DisplayName { get; set; }

    [Required]
    [Display(Name = "Straat")]
    public string Street { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Huisnummer")]
    public string HouseNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Postcode")]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Plaats")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Locatie aanduiding")]
    public string? LocationDetail { get; set; }

    [Phone]
    [Display(Name = "Contactnummer")]
    public string? ContactNumber { get; set; }

    [Display(Name = "Type AED")]
    public string? DeviceType { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Geldigheidsdatum elektroden/batterij")]
    public DateTime? ExpirationDate { get; set; }

    [Display(Name = "Toegangsinstructies / code")]
    [DataType(DataType.MultilineText)]
    public string? AccessInstructions { get; set; }

    [Display(Name = "Laatste instructies")]
    [DataType(DataType.MultilineText)]
    public string? LastMinuteInstructions { get; set; }

    [Display(Name = "Latitude")]
    public double? Latitude { get; set; }

    [Display(Name = "Longitude")]
    public double? Longitude { get; set; }

    [Display(Name = "Actief tonen op kaart")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Beschikbaarheid beperken")]
    public bool LimitAvailability { get; set; }

    public List<DayAvailabilityInput> Availability { get; set; } = DayAvailabilityInput.CreateDefaults();

    public static AedFormViewModel FromEntity(Aed entity)
    {
        var viewModel = new AedFormViewModel
        {
            Id = entity.Id,
            DisplayName = entity.DisplayName,
            Street = entity.Street,
            HouseNumber = entity.HouseNumber,
            PostalCode = entity.PostalCode,
            City = entity.City,
            LocationDetail = entity.LocationDetail,
            ContactNumber = entity.ContactNumber,
            DeviceType = entity.DeviceType,
            ExpirationDate = entity.ExpirationDate,
            AccessInstructions = entity.AccessInstructions,
            LastMinuteInstructions = entity.LastMinuteInstructions,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            IsActive = entity.IsActive,
            LimitAvailability = entity.AvailabilityWindows.Any()
        };

        if (entity.AvailabilityWindows.Any())
        {
            foreach (var day in viewModel.Availability)
            {
                var window = entity.AvailabilityWindows.FirstOrDefault(w => w.DayOfWeek == day.Day);
                if (window is not null)
                {
                    day.IsOpen = true;
                    day.StartTime = window.StartTime;
                    day.EndTime = window.EndTime;
                }
            }
        }

        return viewModel;
    }

    public (Aed aed, List<AedAvailabilityWindow> windows) ToEntity(string ownerId)
    {
        var entity = new Aed
        {
            Id = Id ?? 0,
            OwnerId = ownerId,
            DisplayName = DisplayName?.Trim() ?? string.Empty,
            Street = Street.Trim(),
            HouseNumber = HouseNumber.Trim(),
            PostalCode = PostalCode.Trim().ToUpperInvariant(),
            City = City.Trim(),
            LocationDetail = string.IsNullOrWhiteSpace(LocationDetail) ? null : LocationDetail.Trim(),
            ContactNumber = string.IsNullOrWhiteSpace(ContactNumber) ? null : ContactNumber.Trim(),
            DeviceType = string.IsNullOrWhiteSpace(DeviceType) ? null : DeviceType.Trim(),
            ExpirationDate = ExpirationDate,
            AccessInstructions = string.IsNullOrWhiteSpace(AccessInstructions) ? null : AccessInstructions.Trim(),
            LastMinuteInstructions = string.IsNullOrWhiteSpace(LastMinuteInstructions) ? null : LastMinuteInstructions.Trim(),
            Latitude = Latitude,
            Longitude = Longitude,
            IsActive = IsActive
        };

        var windows = new List<AedAvailabilityWindow>();

        if (LimitAvailability)
        {
            foreach (var slot in Availability.Where(a => a.IsOpen && a.StartTime.HasValue && a.EndTime.HasValue))
            {
                windows.Add(new AedAvailabilityWindow
                {
                    DayOfWeek = slot.Day,
                    StartTime = slot.StartTime!.Value,
                    EndTime = slot.EndTime!.Value
                });
            }
        }

        return (entity, windows);
    }
}

public class DayAvailabilityInput
{
    public DayOfWeek Day { get; set; }

    public bool IsOpen { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan? StartTime { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan? EndTime { get; set; }

    public static List<DayAvailabilityInput> CreateDefaults()
    {
        var days = Enum.GetValues<DayOfWeek>();
        return days.Select(day => new DayAvailabilityInput
        {
            Day = day,
            IsOpen = day != DayOfWeek.Saturday && day != DayOfWeek.Sunday,
            StartTime = day != DayOfWeek.Saturday && day != DayOfWeek.Sunday ? new TimeSpan(9, 0, 0) : null,
            EndTime = day != DayOfWeek.Saturday && day != DayOfWeek.Sunday ? new TimeSpan(17, 0, 0) : null
        }).ToList();
    }
}
