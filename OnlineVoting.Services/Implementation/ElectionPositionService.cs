using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class ElectionPositionService : IElectionPositionService
    {
        private readonly IRepository<ElectionPosition> _electionPositionRepo;
        private readonly IRepository<PositionApplication> _positionApplicationRepo;
        private readonly IRepository<Election> _electionRepo;
        private readonly IRepository<Position> _positionRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public ElectionPositionService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _electionPositionRepo = _unitOfWork.GetRepository<ElectionPosition>();
            _positionApplicationRepo = _unitOfWork.GetRepository<PositionApplication>();
            _electionRepo = _unitOfWork.GetRepository<Election>();
            _positionRepo = _unitOfWork.GetRepository<Position>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateElectionPosition(CreateElectionPositionRequest request)
        {
            _loggerMessage.LogInfo($"Election position creation request received for election {request.ElectionId} and position {request.PositionId}.");

            string electionId = request.ElectionId.Trim();
            string positionId = request.PositionId.Trim();

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == electionId);
            if (election is null)
            {
                _loggerMessage.LogWarn($"Election position creation failed because election with id {electionId} was not found.");
                return Result<string>.NotFound($"Election with id {electionId} was not found.");
            }

            Position? position = await _positionRepo.GetSingleByAsync(x => x.Id == positionId);
            if (position is null)
            {
                _loggerMessage.LogWarn($"Election position creation failed because position with id {positionId} was not found.");
                return Result<string>.NotFound($"Position with id {positionId} was not found.");
            }

            bool electionPositionExists = await _electionPositionRepo.AnyAsync(x => x.ElectionId == electionId && x.PositionId == positionId);
            if (electionPositionExists)
            {
                _loggerMessage.LogWarn($"Election position creation failed because position {positionId} is already assigned to election {electionId}.");
                return Result<string>.Conflict("The position is already assigned to the election.");
            }

            ElectionPosition electionPosition = _mapper.Map<ElectionPosition>(request);

            await _electionPositionRepo.AddAsync(electionPosition);

            _loggerMessage.LogInfo($"Election position {electionPosition.Id} created successfully.");

            return Result<string>.Created("Election position created successfully.");
        }

        public async Task<Result<string>> CreateElectionPositions(CreateElectionPositionsRequest request)
        {
            _loggerMessage.LogInfo($"Bulk election position creation request received for election {request.ElectionId}.");

            string electionId = request.ElectionId.Trim();

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == electionId);
            if (election is null)
            {
                _loggerMessage.LogWarn($"Bulk election position creation failed because election with id {electionId} was not found.");
                return Result<string>.NotFound($"Election with id {electionId} was not found.");
            }

            List<string> positionIds = request.ElectionPositions.Select(x => x.PositionId.Trim()).ToList();

            IQueryable<Position> positionQuery = _positionRepo.GetQueryable(x => positionIds.Contains(x.Id)).AsNoTracking();

            List<string> existingPositionIds = await positionQuery.Select(x => x.Id).ToListAsync();

            HashSet<string> existingPositionIdSet = existingPositionIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            string? missingPositionId = positionIds.FirstOrDefault(positionId => !existingPositionIdSet.Contains(positionId));
            if (missingPositionId is not null)
            {
                _loggerMessage.LogWarn($"Bulk election position creation failed because position with id {missingPositionId} was not found.");
                return Result<string>.NotFound($"Position with id {missingPositionId} was not found.");
            }

            IQueryable<ElectionPosition> existingElectionPositionQuery = _electionPositionRepo.GetQueryable(x => x.ElectionId == electionId 
                && positionIds.Contains(x.PositionId)).AsNoTracking();

            string? existingElectionPositionId = await existingElectionPositionQuery.Select(x => x.PositionId).FirstOrDefaultAsync();
            if (existingElectionPositionId is not null)
            {
                _loggerMessage.LogWarn($"Bulk election position creation failed because position {existingElectionPositionId} is already assigned to election {electionId}.");
                return Result<string>.Conflict("One or more positions are already assigned to the election.");
            }

            List<ElectionPosition> electionPositions = new();

            foreach (CreateElectionPositionItemRequest item in request.ElectionPositions)
            {
                ElectionPosition electionPosition = _mapper.Map<ElectionPosition>(item);
                electionPosition.ElectionId = electionId;

                electionPositions.Add(electionPosition);
            }

            await _electionPositionRepo.AddRangeAsync(electionPositions);

            int electionPositionsCount = electionPositions.Count;

            _loggerMessage.LogInfo($"{electionPositionsCount} election positions created successfully for election {electionId}.");

            return Result<string>.Created($"{electionPositionsCount} election positions created successfully.");
        }

        public async Task<Result<string>> UpdateElectionPosition(CreateElectionPositionRequest request)
        {
            _loggerMessage.LogInfo($"Election position update request received for id {request.Id}.");

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                _loggerMessage.LogWarn("Election position update failed because election position id was not provided.");
                return Result<string>.ValidationError("Election position id is required.");
            }

            string electionPositionId = request.Id.Trim();
            string electionId = request.ElectionId.Trim();
            string positionId = request.PositionId.Trim();

            ElectionPosition? electionPosition = await _electionPositionRepo.GetSingleByAsync(x => x.Id == electionPositionId);
            if (electionPosition is null)
            {
                _loggerMessage.LogWarn($"Election position update failed because election position with id {electionPositionId} was not found.");
                return Result<string>.NotFound($"Election position with id {electionPositionId} was not found.");
            }

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == electionId);
            if (election is null)
            {
                _loggerMessage.LogWarn($"Election position update failed because election with id {electionId} was not found.");
                return Result<string>.NotFound($"Election with id {electionId} was not found.");
            }

            Position? position = await _positionRepo.GetSingleByAsync(x => x.Id == positionId);
            if (position is null)
            {
                _loggerMessage.LogWarn($"Election position update failed because position with id {positionId} was not found.");
                return Result<string>.NotFound($"Position with id {positionId} was not found.");
            }

            bool electionPositionExists = await _electionPositionRepo.AnyAsync(x => x.Id != electionPositionId && x.ElectionId == electionId 
                && x.PositionId == positionId);

            if (electionPositionExists)
            {
                _loggerMessage.LogWarn($"Election position update failed because position with id {positionId} is already assigned to election with id {electionId}.");
                return Result<string>.Conflict($"Position with id {positionId} is already assigned to election with id {electionId}.");
            }

            _mapper.Map(request, electionPosition);

            await _electionPositionRepo.UpdateAsync(electionPosition);

            _loggerMessage.LogInfo($"Election position with id {electionPositionId} updated successfully.");

            return Result<string>.Success("Election position updated successfully.");
        }

        public async Task<Result<string>> ToggleElectionPositionActivation(string id)
        {
            _loggerMessage.LogInfo($"Election position toggle activation request received for id {id}.");

            if (string.IsNullOrWhiteSpace(id))
            {
                _loggerMessage.LogWarn("Election position toggle activation failed because election position id was not provided.");
                return Result<string>.ValidationError("Election position id is required.");
            }

            string electionPositionId = id.Trim();

            ElectionPosition? electionPosition = await _electionPositionRepo.GetSingleByAsync(x => x.Id == electionPositionId);
            if (electionPosition is null)
            {
                _loggerMessage.LogWarn($"Election position toggle activation failed because election position with id {electionPositionId} was not found.");
                return Result<string>.NotFound($"Election position with id {electionPositionId} was not found.");
            }

            electionPosition.Active = !electionPosition.Active;

            await _electionPositionRepo.UpdateAsync(electionPosition);

            string status = electionPosition.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Election position with id {electionPositionId} {status} successfully.");

            return Result<string>.Success($"Election position {status} successfully.");
        }

        public async Task<Result<string>> DeleteElectionPosition(string id)
        {
            _loggerMessage.LogInfo($"Election position deletion request received for id {id}.");
            if (string.IsNullOrWhiteSpace(id))
            {
                _loggerMessage.LogWarn("Election position deletion failed because election position id was not provided.");
                return Result<string>.ValidationError("Election position id is required.");
            }

            string electionPositionId = id.Trim();

            ElectionPosition? electionPosition = await _electionPositionRepo.GetSingleByAsync(x => x.Id == electionPositionId);
            if (electionPosition is null)
            {
                _loggerMessage.LogWarn($"Election position deletion failed because election position with id {electionPositionId} was not found.");
                return Result<string>.NotFound($"Election position with id {electionPositionId} was not found.");
            }

            bool electionPositionInUse = await _positionApplicationRepo.AnyAsync(x => x.ElectionPositionId == electionPositionId);
            if (electionPositionInUse)
            {
                _loggerMessage.LogWarn($"Election position deletion failed because election position with id {electionPositionId} has applications.");
                return Result<string>.Conflict("Election position cannot be deleted because it has applications.");
            }

            await _electionPositionRepo.DeleteAsync(electionPosition);

            _loggerMessage.LogInfo($"Election position with id {electionPositionId} deleted successfully.");

            return Result<string>.Success("Election position deleted successfully.");
        }

        public async Task<Result<PagedResponse<ElectionPositionResponse>>> GetElectionPositions(ElectionPositionRequest request)
        {
            _loggerMessage.LogInfo($"Paged election position list request received for page {request.PageNumber}.");

            IQueryable<ElectionPosition> query = _electionPositionRepo.GetQueryable(include: query => query.Include(x => x.Election)
                .Include(x => x.Position)).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ElectionId))
                query = query.Where(x => x.ElectionId == request.ElectionId);

            if (!string.IsNullOrWhiteSpace(request.PositionId))
                query = query.Where(x => x.PositionId == request.PositionId);

            if (request.ElectionPositionActive.HasValue)
                query = query.Where(x => x.Active == request.ElectionPositionActive.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();
                query = query.Where(x => x.Position.Name.Contains(searchTerm) || x.Election.Name.Contains(searchTerm));
            }

            query = query.OrderBy(x => x.Position.Name);

            PagedList<ElectionPosition> electionPositions = await query.GetPagedItems(request);

            PagedResponse<ElectionPositionResponse> response = _mapper.Map<PagedResponse<ElectionPositionResponse>>(electionPositions);

            List<string> electionPositionIds = electionPositions.Select(x => x.Id).ToList();

            Dictionary<string, int> applicationCounts = new();
                        
            if (electionPositionIds.Count > 0)
            {
                applicationCounts = await _electionPositionRepo.GetQueryable(x => electionPositionIds.Contains(x.Id))
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.Id,
                        NumberOfApplications = x.Applications.Count
                    })
                    .ToDictionaryAsync(x => x.Id, x => x.NumberOfApplications);
            }

            foreach (ElectionPositionResponse electionPosition in response.Items)
            {
                electionPosition.NumberOfApplications =  applicationCounts.GetValueOrDefault(electionPosition.Id);
            }

            _loggerMessage.LogInfo($"{electionPositions.MetaData.TotalCount} election positions found.");

            return Result<PagedResponse<ElectionPositionResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<ElectionPositionWithApplicationsResponse>>> GetElectionPositionsWithApplications(ElectionPositionRequest request)
        {
            _loggerMessage.LogInfo($"Paged election position with applications request received for page {request.PageNumber}.");

            IQueryable<ElectionPosition> query = _electionPositionRepo.GetQueryable(include: query => query.Include(x => x.Election).Include(x => x.Position))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ElectionId))
                query = query.Where(x => x.ElectionId == request.ElectionId);

            if (!string.IsNullOrWhiteSpace(request.PositionId))
                query = query.Where(x => x.PositionId == request.PositionId);

            if (request.ElectionPositionActive.HasValue)
                query = query.Where(x => x.Active == request.ElectionPositionActive.Value);

            if (request.PositionApplicationStatusId.HasValue || request.DepartmentId.HasValue || request.FacultyId.HasValue || request.ApplicationActive.HasValue)
            {
                query = query.Where(x => x.Applications.Any(application =>
                    (!request.PositionApplicationStatusId.HasValue || application.PositionApplicationStatusId == request.PositionApplicationStatusId.Value)
                    && (!request.DepartmentId.HasValue || application.Student.DepartmentId == request.DepartmentId.Value)
                    && (!request.FacultyId.HasValue || application.Student.Department.FacultyId == request.FacultyId.Value)
                    && (!request.ApplicationActive.HasValue || application.Active == request.ApplicationActive.Value)));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();
                query = query.Where(x => x.Position.Name.Contains(searchTerm) || x.Election.Name.Contains(searchTerm));
            }

            query = query.OrderBy(x => x.Position.Name);

            PagedList<ElectionPosition> electionPositions = await query.GetPagedItems(request);

            List<string> electionPositionIds = electionPositions.Select(x => x.Id).ToList();

            List<PositionApplication> applications = [];

            if (electionPositionIds.Count > 0)
            {
                IQueryable<PositionApplication> applicationQuery = _positionApplicationRepo.GetQueryable(x => electionPositionIds.Contains(x.ElectionPositionId),
                    include: query => query.Include(x => x.PositionApplicationStatus).Include(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.Student).ThenInclude(x => x.Department).ThenInclude(x => x.Faculty))
                    .AsNoTracking();

                if (request.PositionApplicationStatusId.HasValue)
                    applicationQuery = applicationQuery.Where(x => x.PositionApplicationStatusId == request.PositionApplicationStatusId.Value);

                if (request.DepartmentId.HasValue)
                    applicationQuery = applicationQuery.Where(x => x.Student.DepartmentId == request.DepartmentId.Value);

                if (request.FacultyId.HasValue)
                    applicationQuery = applicationQuery.Where(x => x.Student.Department.FacultyId == request.FacultyId.Value);

                if (request.ApplicationActive.HasValue)
                    applicationQuery = applicationQuery.Where(x => x.Active == request.ApplicationActive.Value);

                applications = await applicationQuery.ToListAsync();
            }

            Dictionary<string, List<PositionApplication>> applicationsByElectionPosition = applications
                .GroupBy(x => x.ElectionPositionId)
                .ToDictionary(group => group.Key, group => group.ToList());

            PagedResponse<ElectionPositionWithApplicationsResponse> response = _mapper.Map<PagedResponse<ElectionPositionWithApplicationsResponse>>(electionPositions);

            foreach (ElectionPositionWithApplicationsResponse electionPosition in response.Items)
            {
                if (applicationsByElectionPosition.TryGetValue(electionPosition.Id, out List<PositionApplication>? positionApplications))
                {
                    electionPosition.Applications = _mapper.Map<IEnumerable<ElectionPositionApplicationResponse>>(positionApplications);
                    electionPosition.NumberOfApplications = positionApplications.Count;
                }
                else
                {
                    electionPosition.Applications = [];
                    electionPosition.NumberOfApplications = 0;
                }
            }

            _loggerMessage.LogInfo($"{electionPositions.MetaData.TotalCount} election positions with applications found.");

            return Result<PagedResponse<ElectionPositionWithApplicationsResponse>>.Success(response);
        }
    }
}