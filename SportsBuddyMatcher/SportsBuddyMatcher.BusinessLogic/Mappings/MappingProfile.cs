using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LocationRequest, Location>(MemberList.Source);
        CreateMap<Location, LocationDto>();
        CreateMap<UserRequest, User>(MemberList.Source)
            .ForSourceMember(r => r.Password, options => options.DoNotValidate())
            .ForMember(u => u.PasswordHash, options => options.Ignore());
        CreateMap<User, UserDto>();
        CreateMap<PublicSportData, PublicSportDto>();
        CreateMap<PublicProfileData, PublicProfileDto>();
        CreateMap<SkillRequest, Skill>(MemberList.Source);
        CreateMap<Skill, SkillDto>();
        CreateMap<PlayerSkillRequest, PlayerSkill>(MemberList.Source);
        CreateMap<PlayerSkill, PlayerSkillDto>();
        CreateMap<AvailabilityRequest, Availability>(MemberList.Source);
        CreateMap<Availability, AvailabilityDto>();
        CreateMap<EventRequest, Event>(MemberList.Source);
        CreateMap<Event, EventDto>().ForMember(d => d.ParticipantCount, o => o.MapFrom(e => e.UserEvents.Count));
        CreateMap<UserEventRequest, UserEvent>(MemberList.Source);
        CreateMap<UserEvent, UserEventDto>();
    }
}
