using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class FacultyMappingProfile : Profile
    {
        public FacultyMappingProfile() 
        {
            CreateMap<CreateFacultyRequest, Faculty>();

            CreateMap<string, Faculty>()
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source));

            CreateMap<Faculty, FacultyResponse>();

            CreateMap<Department, DepartmentResponse>()
                .ForMember(destination => destination.FacultyName, option => option.MapFrom(source => source.Faculty.Name));
        }
    }
}
