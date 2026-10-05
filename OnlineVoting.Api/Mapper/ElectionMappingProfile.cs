using AutoMapper;
using OnlineVoting.Api.Mapper.CustomResolvers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class ElectionMappingProfile : Profile
    {
        public ElectionMappingProfile()
        {
            CreateMap<CreateElectionTypeRequest, ElectionType>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source.Name.Trim()))
                .ForMember(destination => destination.Description, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim()));

            CreateMap<ElectionType, ElectionTypeResponse>()
                .ForMember(destination => destination.ElectionScopeCode, option => option.MapFrom(source => source.ElectionScope.Code))
                .ForMember(destination => destination.ElectionScope, option => option.MapFrom(source => source.ElectionScope.Name))
                .ForMember(destination => destination.NumberOfElectionPositions, option => option.MapFrom<ElectionPositionCountResolver>())
                .ForMember(destination => destination.NumberOfAspirants, option => option.MapFrom<ElectionAspirantCountResolver>())
                .ForMember(destination => destination.NumberOfContestants, option => option.MapFrom<ElectionContestantCountResolver>());

            CreateMap<ElectionStatus, ElectionStatusResponse>()
                .ForMember(destination => destination.NumberOfElections, option => option.MapFrom<ElectionStatusElectionCountResolver>());

            CreateMap<UpdateElectionStatusRequest, ElectionStatus>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.Code, option => option.Ignore())
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source.Name.Trim()))
                .ForMember(destination => destination.Description, option => option.MapFrom(source => string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim()))
                .ForMember(destination => destination.Active, option => option.Ignore())
                .ForMember(destination => destination.CreatedAt, option => option.Ignore())
                .ForMember(destination => destination.UpdatedAt, option => option.Ignore())
                .ForMember(destination => destination.CreatedBy, option => option.Ignore())
                .ForMember(destination => destination.UpdatedBy, option => option.Ignore())
                .ForMember(destination => destination.Elections, option => option.Ignore());

            CreateMap<CreateElectionRequest, Election>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source.Name.Trim()))
                .ForMember(destination => destination.ApplicationStartAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.ApplicationStartAt) ? (DateTime?)null : DateTime.Parse(source.ApplicationStartAt)))
                .ForMember(destination => destination.ApplicationEndAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.ApplicationEndAt) ? (DateTime?)null : DateTime.Parse(source.ApplicationEndAt)))
                .ForMember(destination => destination.VoterRegistrationStartAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.VoterRegistrationStartAt) ? (DateTime?)null : DateTime.Parse(source.VoterRegistrationStartAt)))
                .ForMember(destination => destination.VoterRegistrationEndAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.VoterRegistrationEndAt) ? (DateTime?)null : DateTime.Parse(source.VoterRegistrationEndAt)))
                .ForMember(destination => destination.VotingStartAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.VotingStartAt) ? (DateTime?)null : DateTime.Parse(source.VotingStartAt)))
                .ForMember(destination => destination.VotingEndAt, option => option.MapFrom(source =>
                    string.IsNullOrWhiteSpace(source.VotingEndAt) ? (DateTime?)null : DateTime.Parse(source.VotingEndAt)));

            CreateMap<Election, ElectionResponse>()
                .ForMember(destination => destination.NumberOfElectionPositions, option => option.MapFrom<ElectionPositionCountForElectionResolver>())
                .ForMember(destination => destination.Year, option => option.MapFrom(source => source.Year.Name))
                .ForMember(destination => destination.ElectionType, option => option.MapFrom(source => source.ElectionType.Name))
                .ForMember(destination => destination.ElectionStatus, option => option.MapFrom(source => source.ElectionStatus.Name))
                .ForMember(destination => destination.FacultyId, option => option.MapFrom(source => source.FacultyId ??
                        (source.Department != null ? source.Department.FacultyId : (long?)null)))
                .ForMember(destination => destination.Faculty, option => option.MapFrom(source => source.Faculty != null
                        ? source.Faculty.Name : source.Department != null ? source.Department.Faculty.Name : null))
                .ForMember(destination => destination.Department, option => option.MapFrom(source => source.Department != null ? source.Department.Name : null))
                .ForMember(destination => destination.ApplicationStartAt, option => option.MapFrom(source => source.ApplicationStartAt.HasValue
                        ? source.ApplicationStartAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(destination => destination.ApplicationEndAt, option => option.MapFrom(source => source.ApplicationEndAt.HasValue
                        ? source.ApplicationEndAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(destination => destination.VoterRegistrationStartAt, option => option.MapFrom(source => source.VoterRegistrationStartAt.HasValue
                        ? source.VoterRegistrationStartAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(destination => destination.VoterRegistrationEndAt, option => option.MapFrom(source => source.VoterRegistrationEndAt.HasValue
                        ? source.VoterRegistrationEndAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(destination => destination.VotingStartAt, option => option.MapFrom(source => source.VotingStartAt.HasValue
                        ? source.VotingStartAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(destination => destination.VotingEndAt, option => option.MapFrom(source => source.VotingEndAt.HasValue
                        ? source.VotingEndAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null));

            CreateMap<ElectionScope, ElectionScopeResponse>();

            CreateMap<UpdateElectionScopeRequest, ElectionScope>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.Code, option => option.Ignore())
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source.Name.Trim()))
                .ForMember(destination => destination.Description, option => option.MapFrom(source => string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim()))
                .ForMember(destination => destination.Active, option => option.Ignore())
                .ForMember(destination => destination.CreatedAt, option => option.Ignore())
                .ForMember(destination => destination.UpdatedAt, option => option.Ignore())
                .ForMember(destination => destination.CreatedBy, option => option.Ignore())
                .ForMember(destination => destination.UpdatedBy, option => option.Ignore())
                .ForMember(destination => destination.ElectionTypes, option => option.Ignore());
        }
    }
}
