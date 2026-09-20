using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class StaffMappingProfile : Profile
    {
        public StaffMappingProfile()
        {
            CreateMap<CreateStaffRequest, Staff>();
        }
    }
}
