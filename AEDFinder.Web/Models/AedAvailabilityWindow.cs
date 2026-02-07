using System.ComponentModel.DataAnnotations;

namespace AEDFinder.Web.Models;

public class AedAvailabilityWindow
{
    public int Id { get; set; }

    [Required]
    public int AedId { get; set; }

    public Aed? Aed { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool Covers(TimeSpan timeOfDay)
    {
        if (StartTime == EndTime)
        {
            return true;
        }

        if (StartTime < EndTime)
        {
            return timeOfDay >= StartTime && timeOfDay <= EndTime;
        }

        return timeOfDay >= StartTime || timeOfDay <= EndTime;
    }
}
