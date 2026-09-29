
using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
namespace OnlineVoting.Api.Mapper
{
    public class ElectionPositionMappingProfile : Profile
    {
        public ElectionPositionMappingProfile()
        {
            CreateMap<ElectionPosition, ElectionPositionResponse>()
                .ForMember(destination => destination.Election, option => option.MapFrom(source => source.Election.Name))
                .ForMember(destination => destination.Position, option => option.MapFrom(source => source.Position.Name))
                .ForMember(destination => destination.NumberOfApplications, option => option.Ignore());

            CreateMap<PositionApplication, PositionApplicationResponse>()
                .ForMember(destination => destination.FirstName, option => option.MapFrom(source => source.Student.User != null
                        ? source.Student.User.FirstName : null))
                .ForMember(destination => destination.LastName, option => option.MapFrom(source => source.Student.User != null
                        ? source.Student.User.LastName : null))
                .ForMember(destination => destination.RegNumber, option => option.MapFrom(source => source.Student.RegNumber))
                .ForMember(destination => destination.DepartmentId, option => option.MapFrom(source => source.Student.DepartmentId))
                .ForMember(destination => destination.Department, option => option.MapFrom(source => source.Student.Department!.Name))
                .ForMember(destination => destination.FacultyId, option => option.MapFrom(source => source.Student.Department!.FacultyId))
                .ForMember(destination => destination.Faculty, option => option.MapFrom(source => source.Student.Department!.Faculty.Name))
                .ForMember(destination => destination.PositionApplicationStatus, option => option.MapFrom(source => source.PositionApplicationStatus.Name));

            CreateMap<ElectionPosition, ElectionPositionWithApplicationsResponse>()
                .ForMember(destination => destination.Election, option => option.MapFrom(source => source.Election.Name))
                .ForMember(destination => destination.Position, option => option.MapFrom(source => source.Position.Name))
                .ForMember(destination => destination.NumberOfApplications, option => option.Ignore())
                .ForMember(destination => destination.Applications, option => option.MapFrom(source => source.Applications));

            CreateMap<CreateElectionPositionRequest, ElectionPosition>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.ElectionId, option => option.MapFrom(source => source.ElectionId.Trim()))
                .ForMember(destination => destination.PositionId, option => option.MapFrom(source => source.PositionId.Trim()))
                .ForMember(destination => destination.Currency, option => option.MapFrom(source => source.Currency.Trim()));

            CreateMap<CreateElectionPositionItemRequest, ElectionPosition>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.ElectionId, option => option.Ignore())
                .ForMember(destination => destination.PositionId, option => option.MapFrom(source => source.PositionId.Trim()))
                .ForMember(destination => destination.Currency, option => option.MapFrom(source => source.Currency.Trim()));
        }
    }
}
