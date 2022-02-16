using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<IdentityModel, User>()
                //.ForMember(c => c.FullName, opt => opt.MapFrom(x => string.Join(' ', x., x.Country)));

        }
    }
}
