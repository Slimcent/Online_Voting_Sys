using AutoMapper;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class ContestantMappingProfile : Profile
    {
        public ContestantMappingProfile()
        {
            CreateMap<Contestant, ContestantResponse>()
                .ForMember(destination => destination.ContestantId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PositionApplicationId, option => option.MapFrom(source => source.PositionApplicationId))
                .ForMember(destination => destination.ContestantName, option => option.MapFrom(source =>
                    source.PositionApplication.Student.User != null
                        ? $"{source.PositionApplication.Student.User.FirstName} {source.PositionApplication.Student.User.LastName}".Trim()
                        : string.Empty))
                .ForMember(destination => destination.RegistrationNumber, option => option.MapFrom(source => source.PositionApplication.Student.RegNumber))
                .ForMember(destination => destination.ElectionName, option => option.MapFrom(source => source.PositionApplication.ElectionPosition.Election.Name))
                .ForMember(destination => destination.PositionName, option => option.MapFrom(source => source.PositionApplication.ElectionPosition.Position.Name));
        }
    }
}
