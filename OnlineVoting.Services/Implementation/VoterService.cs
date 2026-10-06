using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.BackgroundTasks.Implementation;
using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Caching.Keys;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class VoterService : IVoterService
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<RegisteredVoter> _registeredVoterRepo;
        private readonly IRepository<Election> _electionRepo;
        private readonly IRepository<Vote> _voteRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserContext _currentUserContext;

        public VoterService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
            _electionRepo = _unitOfWork.GetRepository<Election>();
            _voteRepo = _unitOfWork.GetRepository<Vote>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
            _cacheService = serviceFactory.GetService<ICacheService>();
            _currentUserContext = serviceFactory.GetService<ICurrentUserContext>();
        }

        public async Task<Result<RegisteredVoterResponse>> RegisterVoter(RegisterVoterRequest request)
        {
            Student? student = await _studentRepo.GetSingleByAsync(x => x.RegNumber == request.RegNumber,
                include: query => query.Include(x => x.Department).ThenInclude(x => x.Faculty).Include(x => x.User),
                tracking: false);

            if (student is null)
            {
                _loggerMessage.LogWarn($"Student with registration number {request.RegNumber} was not found.");
                return Result<RegisteredVoterResponse>.NotFound($"Student with registration number {request.RegNumber} was not found.");
            }

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == request.ElectionId, tracking: false);
            if (election is null)
            {
                _loggerMessage.LogWarn($"Election with id {request.ElectionId} was not found.");
                return Result<RegisteredVoterResponse>.NotFound($"Election with id {request.ElectionId} was not found.");
            }

            if (!election.Active)
            {
                _loggerMessage.LogWarn($"Election {election.Id} is not active.");
                return Result<RegisteredVoterResponse>.Conflict("Voter registration is not available for this election.");
            }

            DateTime utcNow = DateTime.UtcNow;

            if (election.VoterRegistrationStartAt.HasValue && utcNow < election.VoterRegistrationStartAt.Value)
            {
                _loggerMessage.LogWarn($"Voter registration has not started for election {election.Id}.");
                return Result<RegisteredVoterResponse>.Conflict("Voter registration has not started for this election.");
            }

            if (election.VoterRegistrationEndAt.HasValue && utcNow > election.VoterRegistrationEndAt.Value)
            {
                _loggerMessage.LogWarn($"Voter registration has ended for election {election.Id}.");
                return Result<RegisteredVoterResponse>.Conflict("Voter registration has ended for this election.");
            }

            if (election.DepartmentId.HasValue && student.DepartmentId != election.DepartmentId.Value)
            {
                _loggerMessage.LogWarn($"Student {student.Id} is not eligible for department election {election.Id}.");
                return Result<RegisteredVoterResponse>.Forbidden("Student is not eligible to register for this election.");
            }

            if (election.FacultyId.HasValue && student.Department?.FacultyId != election.FacultyId.Value)
            {
                _loggerMessage.LogWarn($"Student {student.Id} is not eligible for faculty election {election.Id}.");
                return Result<RegisteredVoterResponse>.Forbidden("Student is not eligible to register for this election.");
            }

            RegisteredVoter? existingRegisteredVoter = await _registeredVoterRepo.GetSingleByAsync(   x => x.StudentId == student.Id && x.ElectionId == election.Id, tracking: false);

            if (existingRegisteredVoter is not null)
            {
                _loggerMessage.LogWarn($"Student {student.Id} is already registered for election {election.Id}.");
                return Result<RegisteredVoterResponse>.Conflict("Student is already registered for this election.");
            }

            string votingCode = VotingCodeExtention.StudentVotingCode();

            RegisteredVoter registeredVoter = new()
            {
                StudentId = student.Id,
                ElectionId = election.Id,
                VotingCode = votingCode
            };

            await _registeredVoterRepo.AddAsync(registeredVoter);
            await _cacheService.RemoveByTag(CacheTags.RegisteredVoter);

            registeredVoter.Student = student;
            registeredVoter.Election = election;

            VoterEmailDto emailRequest = new()
            {
                Email = student.User!.Email!,
                VotingCode = votingCode,
                FirstName = student.User.FirstName
            };

            try
            {
                _serviceFactory.GetService<IBackgroundTaskQueue>().Enqueue<SendVoterEmailTask, VoterEmailDto>(emailRequest);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Voter email could not be queued for student {student.Id}. {exception.Message}");
            }

            RegisteredVoterResponse response = _mapper.Map<RegisteredVoterResponse>(registeredVoter);

            _loggerMessage.LogInfo($"Student {student.Id} registered successfully for election {election.Id}.");

            return Result<RegisteredVoterResponse>.Created(response);
        }

        public async Task<Result<RegisteredVoterResponse>> GetRegisteredVoter(string registeredVoterId)
        {
            string normalizedRegisteredVoterId = registeredVoterId.Trim();
            string cacheKey = RegisteredVoterCacheKeys.GetRegisteredVoter(normalizedRegisteredVoterId);

            Func<CancellationToken, ValueTask<RegisteredVoterResponse?>> cacheFactory = async _ =>
            {
                RegisteredVoter? registeredVoter = await _registeredVoterRepo.GetSingleByAsync(x => x.Id == normalizedRegisteredVoterId,
                    include: query => query.Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.Election),
                    tracking: false);

                if (registeredVoter is null)
                    return null;

                return _mapper.Map<RegisteredVoterResponse>(registeredVoter);
            };

            RegisteredVoterResponse? response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.RegisteredVoter);

            if (response is null)
            {
                _loggerMessage.LogWarn($"Registered voter with id {normalizedRegisteredVoterId} was not found.");
                return Result<RegisteredVoterResponse>.NotFound($"Registered voter with id {normalizedRegisteredVoterId} was not found.");
            }

            _loggerMessage.LogInfo($"Registered voter with id {normalizedRegisteredVoterId} found.");

            return Result<RegisteredVoterResponse>.Success(response);
        }

        public async Task<Result<PagedResponse<RegisteredVoterResponse>>> GetRegisteredVoters(RegisteredVoterRequest request)
        {
            string cacheKey = RegisteredVoterCacheKeys.GetRegisteredVoters(request);

            Func<CancellationToken, ValueTask<PagedResponse<RegisteredVoterResponse>>> cacheFactory = async _ =>
            {
                IQueryable<RegisteredVoter> query = _registeredVoterRepo.GetQueryable(include: query => query
                    .Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.Election)).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.ElectionId))
                    query = query.Where(x => x.ElectionId == request.ElectionId);

                if (request.StudentId.HasValue)
                    query = query.Where(x => x.StudentId == request.StudentId.Value);

                if (request.Active.HasValue)
                    query = query.Where(x => x.Active == request.Active.Value);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => (x.Student.RegNumber != null && x.Student.RegNumber.Contains(searchTerm))
                        || (x.Student.User != null && x.Student.User.FirstName != null && x.Student.User.FirstName.Contains(searchTerm))
                        || (x.Student.User != null && x.Student.User.LastName != null && x.Student.User.LastName.Contains(searchTerm))
                        || x.Election.Name.Contains(searchTerm));
                }

                query = query.OrderByDescending(x => x.CreatedAt);

                PagedList<RegisteredVoter> registeredVoters = await query.GetPagedItems(request);

                PagedResponse<RegisteredVoterResponse> response = _mapper.Map<PagedResponse<RegisteredVoterResponse>>(registeredVoters);

                _loggerMessage.LogInfo($"{registeredVoters.MetaData.TotalCount} registered voters found.");

                return response;
            };

            PagedResponse<RegisteredVoterResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.RegisteredVoter);

            return Result<PagedResponse<RegisteredVoterResponse>>.Success(response);
        }

        public async Task<Result<string>> CastVote(CastVoteRequest request)
        {
            string registeredVoterId = request.RegisteredVoterId.Trim();
            string votingCode = request.VotingCode.Trim();
            string electionPositionId = request.ElectionPositionId.Trim();
            string contestantId = request.ContestantId.Trim();

            string? userId = _currentUserContext.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Vote casting rejected because the authenticated user could not be identified.");
                return Result<string>.Unauthorized("User is not authenticated.");
            }

            RegisteredVoter? registeredVoter = await _registeredVoterRepo.GetSingleByAsync(x => x.Id == registeredVoterId && x.VotingCode == votingCode,
                include: query => query.Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.Election), tracking: false);

            if (registeredVoter is null)
            {
                _loggerMessage.LogWarn("Invalid voter credentials provided.");
                return Result<string>.Forbidden("Invalid voter credentials.");
            }

            if (registeredVoter.Student.UserId != userId)
            {
                _loggerMessage.LogWarn($"User {userId} attempted to vote with voter credentials that do not belong to them.");
                return Result<string>.Forbidden("The voter credentials do not belong to the authenticated user.");
            }

            if (!registeredVoter.Active)
            {
                _loggerMessage.LogWarn($"Registered voter with id {registeredVoter.Id} is inactive.");
                return Result<string>.Forbidden("Registered voter is not permitted to vote.");
            }

            IRepository<ElectionPosition> electionPositionRepository = _unitOfWork.GetRepository<ElectionPosition>();
            ElectionPosition? electionPosition = await electionPositionRepository.GetSingleByAsync(x => x.Id == electionPositionId,
                include: query => query.Include(x => x.Election).Include(x => x.Position), tracking: false);

            if (electionPosition is null)
            {
                _loggerMessage.LogWarn($"Election position with id {electionPositionId} was not found.");
                return Result<string>.NotFound($"Election position with id {electionPositionId} was not found.");
            }

            if (!electionPosition.Active)
            {
                _loggerMessage.LogWarn($"Election position with id {electionPositionId} is inactive.");
                return Result<string>.Conflict("Voting is not available for this election position.");
            }

            if (registeredVoter.ElectionId != electionPosition.ElectionId)
            {
                _loggerMessage.LogWarn($"Registered voter with id {registeredVoter.Id} is not eligible to vote in election {electionPosition.ElectionId}.");
                return Result<string>.Forbidden("Registered voter is not eligible to vote in this election.");
            }

            if (!electionPosition.Election.Active)
            {
                _loggerMessage.LogWarn($"Election with id {electionPosition.ElectionId} is inactive.");
                return Result<string>.Conflict("Voting is not available for this election.");
            }

            if (!electionPosition.Election.VotingStartAt.HasValue || !electionPosition.Election.VotingEndAt.HasValue)
            {
                _loggerMessage.LogWarn($"Voting period is not configured for election {electionPosition.ElectionId}.");
                return Result<string>.Conflict("Voting period is not configured for this election.");
            }

            DateTime now = DateTime.UtcNow;

            if (now < electionPosition.Election.VotingStartAt.Value)
            {
                _loggerMessage.LogWarn($"Voting has not started for election {electionPosition.ElectionId}.");
                return Result<string>.Conflict("Voting has not started for this election.");
            }

            if (now > electionPosition.Election.VotingEndAt.Value)
            {
                _loggerMessage.LogWarn($"Voting has ended for election {electionPosition.ElectionId}.");
                return Result<string>.Conflict("Voting has ended for this election.");
            }

            IRepository<Contestant> contestantRepository = _unitOfWork.GetRepository<Contestant>();
            Contestant? contestant = await contestantRepository.GetSingleByAsync(x => x.Id == contestantId, include: query => query.Include(x => x.PositionApplication),
                tracking: false);

            if (contestant is null)
            {
                _loggerMessage.LogWarn($"Contestant with id {contestantId} was not found.");
                return Result<string>.NotFound($"Contestant with id {contestantId} was not found.");
            }

            if (!contestant.Active)
            {
                _loggerMessage.LogWarn($"Contestant with id {contestantId} is inactive.");
                return Result<string>.Conflict("The selected contestant is not available for voting.");
            }

            if (contestant.PositionApplication.ElectionPositionId != electionPositionId)
            {
                _loggerMessage.LogWarn($"Contestant with id {contestantId} does not belong to election position {electionPositionId}.");
                return Result<string>.ValidationError("The selected contestant does not belong to this election position.");
            }

            Vote? existingVote = await _voteRepo.GetSingleByAsync(x => x.RegisteredVoterId == registeredVoter.Id && x.ElectionPositionId == electionPositionId);
            if (existingVote is not null)
            {
                _loggerMessage.LogWarn($"Registered voter with id {registeredVoter.Id} has already voted for election position {electionPositionId}.");
                return Result<string>.Conflict("A vote has already been cast for this election position.");
            }

            Vote vote = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = contestantId,
                ElectionPositionId = electionPositionId,
                VotedAt = DateTime.UtcNow
            };

            await _voteRepo.AddAsync(vote);
            await _cacheService.RemoveByTag(CacheTags.VoteHistory);
            await _cacheService.RemoveByTag(CacheTags.ElectionResult);

            try
            {
                VoteConfirmationEmailRequest emailRequest = new()
                {
                    Email = registeredVoter.Student.User!.Email!,
                    FirstName = registeredVoter.Student.User.FirstName!,
                    ElectionName = electionPosition.Election.Name,
                    PositionName = electionPosition.Position.Name,
                    VotedAt = vote.VotedAt
                };

                _serviceFactory.GetService<IBackgroundTaskQueue>().Enqueue<SendVoteConfirmationEmailTask, VoteConfirmationEmailRequest>(emailRequest);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Vote confirmation email could not be queued for registered voter {registeredVoter.Id}. Error: {exception.Message}");
            }

            _loggerMessage.LogInfo($"Vote successfully cast for election position {electionPositionId}.");

            return Result<string>.Created("Vote cast successfully.");
        }

        public async Task<Result<PagedResponse<VoteHistoryResponse>>> GetMyVotes(VoteHistoryRequest request)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Vote history retrieval rejected because the authenticated user could not be identified.");
                return Result<PagedResponse<VoteHistoryResponse>>.Unauthorized("User is not authenticated.");
            }

            string cacheKey = VoteHistoryCacheKeys.GetMyVotes(userId, request);

            Func<CancellationToken, ValueTask<PagedResponse<VoteHistoryResponse>>> cacheFactory = async _ =>
            {
                IQueryable<Vote> query = _voteRepo.GetQueryable(x => x.RegisteredVoter.Student.UserId == userId,
                    include: query => query.Include(x => x.RegisteredVoter).ThenInclude(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.Contestant).ThenInclude(x => x.PositionApplication).ThenInclude(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Position)).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.ElectionId))
                    query = query.Where(x => x.ElectionPosition.ElectionId == request.ElectionId);

                if (!string.IsNullOrWhiteSpace(request.ElectionPositionId))
                    query = query.Where(x => x.ElectionPositionId == request.ElectionPositionId);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.ElectionPosition.Election.Name.Contains(searchTerm)
                        || x.ElectionPosition.Position.Name.Contains(searchTerm)
                        || (x.Contestant.PositionApplication.Student.User != null
                            && x.Contestant.PositionApplication.Student.User.FirstName != null
                            && x.Contestant.PositionApplication.Student.User.FirstName.Contains(searchTerm))
                        || (x.Contestant.PositionApplication.Student.User != null
                            && x.Contestant.PositionApplication.Student.User.LastName != null
                            && x.Contestant.PositionApplication.Student.User.LastName.Contains(searchTerm)));
                }

                query = query.OrderByDescending(x => x.VotedAt);

                PagedList<Vote> votes = await query.GetPagedItems(request);

                PagedResponse<VoteHistoryResponse> response = _mapper.Map<PagedResponse<VoteHistoryResponse>>(votes);

                _loggerMessage.LogInfo($"{votes.MetaData.TotalCount} votes found for user {userId}.");

                return response;
            };

            PagedResponse<VoteHistoryResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.VoteHistory);

            return Result<PagedResponse<VoteHistoryResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<ElectionResultResponse>>> GetElectionResults(ElectionResultRequest request)
        {
            string cacheKey = ElectionResultCacheKeys.GetElectionResults(request);

            Func<CancellationToken, ValueTask<PagedResponse<ElectionResultResponse>>> cacheFactory = async _ =>
            {
                DateTime now = DateTime.UtcNow;

                IRepository<Contestant> contestantRepository = _unitOfWork.GetRepository<Contestant>();

                IQueryable<Contestant> query = contestantRepository.GetQueryable(x => x.Active
                    && x.PositionApplication.ElectionPosition.Election.VotingEndAt.HasValue
                    && x.PositionApplication.ElectionPosition.Election.VotingEndAt.Value <= now,
                    include: query => query.Include(x => x.PositionApplication).ThenInclude(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition).ThenInclude(x => x.Position)).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.ElectionId))
                {
                    string electionId = request.ElectionId.Trim();
                    query = query.Where(x => x.PositionApplication.ElectionPosition.ElectionId == electionId);
                }

                if (!string.IsNullOrWhiteSpace(request.ElectionPositionId))
                {
                    string electionPositionId = request.ElectionPositionId.Trim();
                    query = query.Where(x => x.PositionApplication.ElectionPositionId == electionPositionId);
                }

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.PositionApplication.ElectionPosition.Election.Name.Contains(searchTerm)
                        || x.PositionApplication.ElectionPosition.Position.Name.Contains(searchTerm)
                        || (x.PositionApplication.Student.User != null
                            && x.PositionApplication.Student.User.FirstName != null
                            && x.PositionApplication.Student.User.FirstName.Contains(searchTerm))
                        || (x.PositionApplication.Student.User != null
                            && x.PositionApplication.Student.User.LastName != null
                            && x.PositionApplication.Student.User.LastName.Contains(searchTerm)));
                }

                query = query.OrderBy(x => x.PositionApplication.ElectionPosition.Position.Name)
                    .ThenBy(x => x.PositionApplication.Student.User!.FirstName)
                    .ThenBy(x => x.PositionApplication.Student.User!.LastName);

                PagedList<Contestant> contestants = await query.GetPagedItems(request);

                List<string> contestantIds = contestants.Select(x => x.Id).ToList();
                List<string> electionPositionIds = contestants.Select(x => x.PositionApplication.ElectionPositionId).Distinct().ToList();

                IQueryable<Vote> voteQuery = _voteRepo.GetQueryable(x => electionPositionIds.Contains(x.ElectionPositionId)).AsNoTracking();

                Dictionary<string, int> contestantVoteCounts = await voteQuery.Where(x => contestantIds.Contains(x.ContestantId))
                    .GroupBy(x => x.ContestantId)
                    .ToDictionaryAsync(x => x.Key, x => x.Count());

                Dictionary<string, int> positionVoteCounts = await voteQuery.GroupBy(x => x.ElectionPositionId)
                    .ToDictionaryAsync(x => x.Key, x => x.Count());

                List<ElectionResultResponse> items = contestants.Select(contestant =>
                {
                    int voteCount = contestantVoteCounts.GetValueOrDefault(contestant.Id);
                    int totalVotes = positionVoteCounts.GetValueOrDefault(contestant.PositionApplication.ElectionPositionId);
                    decimal percentage = totalVotes == 0 ? 0 : Math.Round((decimal)voteCount / totalVotes * 100, 2);

                    return new ElectionResultResponse
                    {
                        ElectionId = contestant.PositionApplication.ElectionPosition.ElectionId,
                        ElectionName = contestant.PositionApplication.ElectionPosition.Election.Name,
                        ElectionPositionId = contestant.PositionApplication.ElectionPositionId,
                        PositionName = contestant.PositionApplication.ElectionPosition.Position.Name,
                        ContestantId = contestant.Id,
                        ContestantName = $"{contestant.PositionApplication.Student.User!.FirstName} {contestant.PositionApplication.Student.User.LastName}",
                        VoteCount = voteCount,
                        TotalVotes = totalVotes,
                        Percentage = percentage
                    };
                }).ToList();

                PagedResponse<ElectionResultResponse> response = new()
                {
                    Items = items,
                    MetaData = contestants.MetaData
                };

                _loggerMessage.LogInfo($"{contestants.MetaData.TotalCount} election result records found.");

                return response;
            };

            PagedResponse<ElectionResultResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.ElectionResult);

            return Result<PagedResponse<ElectionResultResponse>>.Success(response);
        }
    }
}