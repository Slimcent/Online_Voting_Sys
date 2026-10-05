using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class ElectionTypeService : IElectionTypeService
    {
        private readonly IRepository<ElectionType> _electionTypeRepo;
        private readonly IRepository<Election> _electionRepo;
        private readonly IRepository<ElectionScope> _electionScopeRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public ElectionTypeService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _electionScopeRepo = _unitOfWork.GetRepository<ElectionScope>();
            _electionTypeRepo = _unitOfWork.GetRepository<ElectionType>();
            _electionRepo = _unitOfWork.GetRepository<Election>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateElectionType(CreateElectionTypeRequest request)
        {
            _loggerMessage.LogInfo($"Election type creation request received for {request.Name}.");
            if (request.Id.HasValue)
            {
                _loggerMessage.LogWarn("Election type creation failed because ID was provided.");
                return Result<string>.ValidationError("The ID is invalid.");
            }

            ElectionScope? electionScope = await _electionScopeRepo.GetByIdAsync(request.ElectionScopeId);
            if (electionScope is null)
            {
                _loggerMessage.LogWarn($"Election type creation failed because election scope with id {request.ElectionScopeId} was not found.");
                return Result<string>.NotFound($"Election scope with id {request.ElectionScopeId} was not found.");
            }

            if (!electionScope.Active)
            {
                _loggerMessage.LogWarn($"Election type creation failed because election scope with id {request.ElectionScopeId} is inactive.");
                return Result<string>.ValidationError($"Election scope with id {request.ElectionScopeId} is inactive.");
            }

            string electionTypeName = request.Name.Trim();

            bool electionTypeExists = await _electionTypeRepo.AnyAsync(x => x.Name == electionTypeName);
            if (electionTypeExists)
            {
                _loggerMessage.LogWarn($"Election type creation failed because election type {electionTypeName} already exists.");
                return Result<string>.Conflict($"Election type with name {electionTypeName} already exists.");
            }

            ElectionType electionType = _mapper.Map<ElectionType>(request);

            await _electionTypeRepo.AddAsync(electionType);

            _loggerMessage.LogInfo($"Election type {electionType.Name} created successfully.");

            return Result<string>.Created($"Election type with name {electionType.Name} created successfully.");
        }

        public async Task<Result<IEnumerable<ElectionTypeResponse>>> GetElectionTypes()
        {
            _loggerMessage.LogInfo("Election type list request received.");

            IQueryable<ElectionType> query = _electionTypeRepo.GetQueryable(include: query => query.Include(x => x.ElectionScope).Include(x => x.Elections)
                .ThenInclude(x => x.ElectionPositions).ThenInclude(x => x.Applications).ThenInclude(x => x.Contestant)).AsNoTracking()
            .OrderBy(x => x.Name);

            List<ElectionType> electionTypes = await query.ToListAsync();

            IEnumerable<ElectionTypeResponse> response = _mapper.Map<IEnumerable<ElectionTypeResponse>>(electionTypes);

            _loggerMessage.LogInfo($"{electionTypes.Count} election types found.");

            return Result<IEnumerable<ElectionTypeResponse>>.Success(response);
        }

        public async Task<Result<ElectionTypeResponse>> GetElectionType(int id)
        {
            _loggerMessage.LogInfo($"Election type request received for id {id}.");

            ElectionType? electionType = await _electionTypeRepo.GetSingleByAsync(x => x.Id == id, include: query => query.Include(x => x.ElectionScope)
                .Include(x => x.Elections).ThenInclude(x => x.ElectionPositions).ThenInclude(x => x.Applications).ThenInclude(x => x.Contestant));

            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election type with id {id} was not found.");
                return Result<ElectionTypeResponse>.NotFound($"Election type with id {id} was not found.");
            }

            ElectionTypeResponse response = _mapper.Map<ElectionTypeResponse>(electionType);

            _loggerMessage.LogInfo($"Election type with id {id} found successfully.");

            return Result<ElectionTypeResponse>.Success(response);
        }

        public async Task<Result<string>> UpdateElectionType(CreateElectionTypeRequest request)
        {
            _loggerMessage.LogInfo($"Election type update request received for id {request.Id}.");
            if (!request.Id.HasValue)
            {
                _loggerMessage.LogWarn("Election type update failed because ID was not provided.");
                return Result<string>.ValidationError("The ID is required.");
            }

            ElectionType? electionType = await _electionTypeRepo.GetByIdAsync(request.Id.Value);
            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election type update failed because election type with id {request.Id.Value} was not found.");
                return Result<string>.NotFound($"Election type with id {request.Id.Value} was not found.");
            }

            ElectionScope? electionScope = await _electionScopeRepo.GetByIdAsync(request.ElectionScopeId);
            if (electionScope is null)
            {
                _loggerMessage.LogWarn($"Election type update failed because election scope with id {request.ElectionScopeId} was not found.");
                return Result<string>.NotFound($"Election scope with id {request.ElectionScopeId} was not found.");
            }

            if (electionType.ElectionScopeId != request.ElectionScopeId && !electionScope.Active)
            {
                _loggerMessage.LogWarn($"Election type update failed because election scope with id {request.ElectionScopeId} is inactive.");
                return Result<string>.ValidationError($"Election scope with id {request.ElectionScopeId} is inactive.");
            }

            string electionTypeName = request.Name.Trim();

            bool electionTypeExists = await _electionTypeRepo.AnyAsync(x => x.Id != request.Id.Value && x.Name == electionTypeName);
            if (electionTypeExists)
            {
                _loggerMessage.LogWarn($"Election type update failed because election type {electionTypeName} already exists.");
                return Result<string>.Conflict($"Election type with name {electionTypeName} already exists.");
            }

            _mapper.Map(request, electionType);

            await _electionTypeRepo.UpdateAsync(electionType);

            _loggerMessage.LogInfo($"Election type with id {request.Id.Value} updated successfully.");

            return Result<string>.Success("Election type updated successfully.");
        }

        public async Task<Result<string>> ToggleElectionTypeActivation(int id)
        {
            _loggerMessage.LogInfo($"Election type toggle activation request received for id {id}.");

            ElectionType? electionType = await _electionTypeRepo.GetByIdAsync(id);
            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election type toggle activation failed because election type with id {id} was not found.");
                return Result<string>.NotFound($"Election type with id {id} was not found.");
            }

            electionType.Active = !electionType.Active;

            await _electionTypeRepo.UpdateAsync(electionType);
            string status = electionType.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Election type with id {id} {status} successfully.");

            return Result<string>.Success($"Election type {status} successfully.");
        }

        public async Task<Result<string>> DeleteElectionType(int id)
        {
            _loggerMessage.LogInfo($"Election type deletion request received for id {id}.");

            ElectionType? electionType = await _electionTypeRepo.GetByIdAsync(id);
            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election type deletion failed because election type with id {id} was not found.");
                return Result<string>.NotFound($"Election type with id {id} was not found.");
            }

            bool electionTypeInUse = await _electionRepo.AnyAsync(x => x.ElectionTypeId == id);
            if (electionTypeInUse)
            {
                _loggerMessage.LogWarn($"Election type deletion failed because election type with id {id} is already in use.");

                return Result<string>.Conflict("Election type cannot be deleted because it is already used by an election.");
            }

            await _electionTypeRepo.DeleteAsync(electionType);

            _loggerMessage.LogInfo($"Election type with id {id} deleted successfully.");

            return Result<string>.Success("Election type deleted successfully.");
        }

        public async Task<Result<PagedResponse<ElectionTypeResponse>>> GetPagedElectionTypes(ElectionTypeRequest request)
        {
            _loggerMessage.LogInfo($"Paginated election type list request received for page {request.PageNumber}.");

            IQueryable<ElectionType> query = _electionTypeRepo.GetQueryable(include: query => query.Include(x => x.Elections)
                .ThenInclude(x => x.ElectionPositions).ThenInclude(x => x.Applications).ThenInclude(x => x.Contestant))
                .Include(x => x.ElectionScope).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(x => x.Name.Contains(request.SearchTerm.Trim()));

            query = query.OrderBy(x => x.Name);

            PagedList<ElectionType> electionTypes = await query.GetPagedItems(request);

            PagedResponse<ElectionTypeResponse> response = _mapper.Map<PagedResponse<ElectionTypeResponse>>(electionTypes);

            _loggerMessage.LogInfo($"{electionTypes.MetaData.TotalCount} election types found.");

            return Result<PagedResponse<ElectionTypeResponse>>.Success(response);
        }
    }
}