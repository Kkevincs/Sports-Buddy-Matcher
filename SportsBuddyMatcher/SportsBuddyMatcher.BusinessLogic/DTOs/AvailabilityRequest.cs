using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class AvailabilityRequest : IValidatableObject
{
    [Range(0, 6)]
    public required DayOfWeek DayOfWeek { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
            yield return new ValidationResult("EndTime must be after StartTime on the same day.", new[] { nameof(EndTime) });
    }
}
