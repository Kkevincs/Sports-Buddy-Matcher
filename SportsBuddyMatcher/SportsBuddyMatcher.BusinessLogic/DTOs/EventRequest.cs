using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class EventRequest : IValidatableObject
{
    [Required, MaxLength(255)]
    public required string Name { get; set; }

    public required DateTime Date { get; set; }
    public required DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public required int SkillId { get; set; }

    [Range(1, 5)]
    public required int SkillLevel { get; set; }

    public required int Location { get; set; }

    [Range(1, int.MaxValue)]
    public required int MaxParticipants { get; set; }
    [MaxLength(4000)]
    public string? Bio { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= Date)
            yield return new ValidationResult("EndDate must be after Date.", new[] { nameof(EndDate) });
    }
}
