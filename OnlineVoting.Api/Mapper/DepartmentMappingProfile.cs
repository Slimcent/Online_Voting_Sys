using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class DepartmentMappingProfile : Profile
    {
        public DepartmentMappingProfile() 
        {
            CreateMap<Department, DepartmentResponse>()
            .ForMember(destination => destination.FacultyName, option => option.MapFrom(source => source.Faculty.Name));

            CreateMap<CreateDepartmentRequest, Department>();

            CreateMap<string, Department>()
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source));
        }
    }
}
