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

            CreateMap<Vote, VoteHistoryResponse>()
                .ForMember(destination => destination.VoteId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.ElectionId, option => option.MapFrom(source => source.ElectionPosition.ElectionId))
                .ForMember(destination => destination.ElectionName, option => option.MapFrom(source => source.ElectionPosition.Election.Name))
                .ForMember(destination => destination.ElectionPositionId, option => option.MapFrom(source => source.ElectionPositionId))
                .ForMember(destination => destination.PositionName, option => option.MapFrom(source => source.ElectionPosition.Position.Name))
                .ForMember(destination => destination.ContestantId, option => option.MapFrom(source => source.ContestantId))
                .ForMember(destination => destination.ContestantName, option => option.MapFrom(source =>
                    $"{source.Contestant.PositionApplication.Student.User!.FirstName} {source.Contestant.PositionApplication.Student.User.LastName}"));
        }
    }
}