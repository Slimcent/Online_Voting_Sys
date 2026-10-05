using AutoMapper;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper.CustomResolvers
{
    public class ElectionPositionCountResolver : IValueResolver<ElectionType, ElectionTypeResponse, int>
    {
        public int Resolve(ElectionType source, ElectionTypeResponse destination, int destMember, ResolutionContext context)
        {
            int resolvedValue = source.Elections.SelectMany(election => election.ElectionPositions)
                .Count();

            return resolvedValue;
        }
    }

    public class ElectionAspirantCountResolver : IValueResolver<ElectionType, ElectionTypeResponse, int>
    {
        public int Resolve(ElectionType source, ElectionTypeResponse destination, int destMember, ResolutionContext context)
        {
            int resolvedValue = source.Elections.SelectMany(election => election.ElectionPositions)
                .SelectMany(electionPosition => electionPosition.Applications)
                .Count();

            return resolvedValue;
        }
    }

    public class ElectionContestantCountResolver : IValueResolver<ElectionType, ElectionTypeResponse, int>
    {
        public int Resolve(ElectionType source, ElectionTypeResponse destination, int destMember, ResolutionContext context)
        {
            int resolvedValue = source.Elections.SelectMany(election => election.ElectionPositions)
                .SelectMany(electionPosition => electionPosition.Applications)
                .Count(application => application.Contestant is not null);

            return resolvedValue;
        }
    }

    public class ElectionStatusElectionCountResolver : IValueResolver<ElectionStatus, ElectionStatusResponse, int>
    {
        public int Resolve(ElectionStatus source, ElectionStatusResponse destination, int destMember, ResolutionContext context)
        {
            int resolvedValue = source.Elections.Count;

            return resolvedValue;
        }
    }

    public class ElectionPositionCountForElectionResolver : IValueResolver<Election, ElectionResponse, int>
    {
        public int Resolve(Election source, ElectionResponse destination, int destMember, ResolutionContext context)
        {
            int resolvedValue = source.ElectionPositions.Count;

            return resolvedValue;
        }
    }
}