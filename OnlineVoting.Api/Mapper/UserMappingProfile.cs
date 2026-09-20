using AutoMapper;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, CreateUserEmailRequest>()
                .ForMember(dest => dest.UserId, options => options.MapFrom(source => source.Id))
                .ForMember(dest => dest.EmailConfirmationToken, options => options.Ignore())
                .ForMember(dest => dest.ResetPasswordToken, options => options.Ignore());
        }
    }
}
