using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using AutoMapper;

namespace OnlineVoting.Api.Mapper
{
    public class VotingMappingProfile : Profile
    {
        public VotingMappingProfile()
        {
            CreateMap<RegisteredVoter, RegisteredVoterResponse>()
                .ForMember(destination => destination.RegisteredVoterId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.RegistrationNumber, option => option.MapFrom(source => source.Student.RegNumber))
                .ForMember(destination => destination.StudentName, option => option.MapFrom(source => $"{source.Student.User!.FirstName} {source.Student.User.LastName}"))
                .ForMember(destination => destination.ElectionName, option => option.MapFrom(source => source.Election.Name));
        }
    }
}
