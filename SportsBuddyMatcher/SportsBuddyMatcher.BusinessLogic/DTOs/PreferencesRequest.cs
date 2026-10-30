using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class PreferencesRequest
{
    [Range(0, int.MaxValue)]
    public decimal? PreferredRangeKm { get; set; }
}
