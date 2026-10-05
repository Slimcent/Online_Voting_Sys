using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
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
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;
        private readonly ICacheService _cacheService;

        public VoterService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
            _electionRepo = _unitOfWork.GetRepository<Election>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
            _cacheService = serviceFactory.GetService<ICacheService>();
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

            RegisteredVoter? existingRegisteredVoter = await _registeredVoterRepo.GetSingleByAsync(
                x => x.StudentId == student.Id && x.ElectionId == election.Id, tracking: false);

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
    }
}