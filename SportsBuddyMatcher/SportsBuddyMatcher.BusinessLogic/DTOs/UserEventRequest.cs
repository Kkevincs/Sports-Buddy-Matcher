using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class UserEventRequest
{
    public required int EventId { get; set; }
}
