using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class RefreshTokenMappingProfile : Profile
    {
        public RefreshTokenMappingProfile() 
        {
            CreateMap<RefreshTokenContext, RefreshToken>()
                .ForMember(destination => destination.TokenHash, option => option.Ignore())
                .ForMember(destination => destination.FamilyId, option => option.Ignore())
                .ForMember(destination => destination.CreatedAt, option => option.Ignore())
                .ForMember(destination => destination.ExpiresAt, option => option.Ignore())
                .ForMember(destination => destination.FamilyExpiresAt, option => option.Ignore())
                .ForMember(destination => destination.RevokedAt, option => option.Ignore())
                .ForMember(destination => destination.ReplacedByTokenHash, option => option.Ignore())
                .ForMember(destination => destination.RevokedReason, option => option.Ignore())
                .ForMember(destination => destination.CreatedByIp, option => option.Ignore())
                .ForMember(destination => destination.RevokedByIp, option => option.Ignore())
                .ForMember(destination => destination.UserAgent, option => option.Ignore())
                .ForMember(destination => destination.User, option => option.Ignore())
                .ForMember(destination => destination.RowVersion, option => option.Ignore());
        }
    }
}
