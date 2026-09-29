using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Extensions;
using VotingSystem.Data.Implementation;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class ElectionService : IElectionService
    {
        private readonly IRepository<ElectionStatus> _electionStatusRepo;
        private readonly IRepository<ElectionScope> _electionScopeRepo;
        private readonly IRepository<Election> _electionRepo;
        private readonly IRepository<Year> _yearRepo;
        private readonly IRepository<ElectionType> _electionTypeRepo;
        private readonly IRepository<Faculty> _facultyRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public ElectionService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _electionScopeRepo = _unitOfWork.GetRepository<ElectionScope>();
            _electionStatusRepo = _unitOfWork.GetRepository<ElectionStatus>();
            _electionRepo = _unitOfWork.GetRepository<Election>();
            _yearRepo = _unitOfWork.GetRepository<Year>();
            _electionTypeRepo = _unitOfWork.GetRepository<ElectionType>();
            _facultyRepo = _unitOfWork.GetRepository<Faculty>();
            _departmentRepo = _unitOfWork.GetRepository<Department>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<IEnumerable<ElectionStatusResponse>>> GetElectionStatuses()
        {
            _loggerMessage.LogInfo("Election status list request received.");

            IQueryable<ElectionStatus> query = _electionStatusRepo.GetQueryable(include: query => query.Include(x => x.Elections)).AsNoTracking()
                .OrderBy(x => x.Name);

            List<ElectionStatus> electionStatuses = await query.ToListAsync();

            IEnumerable<ElectionStatusResponse> response =  _mapper.Map<IEnumerable<ElectionStatusResponse>>(electionStatuses);

            _loggerMessage.LogInfo($"{electionStatuses.Count} election statuses found.");

            return Result<IEnumerable<ElectionStatusResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<ElectionStatusResponse>>> GetPagedElectionStatuses(ElectionStatusRequest request)
        {
            _loggerMessage.LogInfo($"Paginated election status list request received for page {request.PageNumber}.");

            IQueryable<ElectionStatus> query = _electionStatusRepo.GetQueryable(include: query => query.Include(x => x.Elections)).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(x => x.Name.Contains(request.SearchTerm.Trim()));

            query = query.OrderBy(x => x.Name);

            PagedList<ElectionStatus> electionStatuses = await query.GetPagedItems(request);

            PagedResponse<ElectionStatusResponse> response =  _mapper.Map<PagedResponse<ElectionStatusResponse>>(electionStatuses);

            _loggerMessage.LogInfo($"{electionStatuses.MetaData.TotalCount} election statuses found.");

            return Result<PagedResponse<ElectionStatusResponse>>.Success(response);
        }

        public async Task<Result<ElectionStatusResponse>> GetElectionStatus(int id)
        {
            _loggerMessage.LogInfo($"Election status request received for id {id}.");

            ElectionStatus? electionStatus = await _electionStatusRepo.GetSingleByAsync(x => x.Id == id, include: query => query.Include(x => x.Elections));
            if (electionStatus is null)
            {
                _loggerMessage.LogWarn($"Election status with id {id} was not found.");
                return Result<ElectionStatusResponse>.NotFound($"Election status with id {id} was not found.");
            }

            ElectionStatusResponse response = _mapper.Map<ElectionStatusResponse>(electionStatus);

            _loggerMessage.LogInfo($"Election status with id {id} found successfully.");

            return Result<ElectionStatusResponse>.Success(response);
        }

        public async Task<Result<string>> UpdateElectionStatus(UpdateElectionStatusRequest request)
        {
            _loggerMessage.LogInfo($"Election status update request received for id {request.Id}.");

            ElectionStatus? electionStatus = await _electionStatusRepo.GetByIdAsync(request.Id);
            if (electionStatus is null)
            {
                _loggerMessage.LogWarn($"Election status update failed because election status with id {request.Id} was not found.");
                return Result<string>.NotFound($"Election status with id {request.Id} was not found.");
            }

            string electionStatusName = request.Name.Trim();
            bool electionStatusExists = await _electionStatusRepo.AnyAsync(x => x.Id != request.Id && x.Name == electionStatusName);

            if (electionStatusExists)
            {
                _loggerMessage.LogWarn($"Election status update failed because election status {electionStatusName} already exists.");
                return Result<string>.Conflict($"Election status with name {electionStatusName} already exists.");
            }

            _mapper.Map(request, electionStatus);

            await _electionStatusRepo.UpdateAsync(electionStatus);

            _loggerMessage.LogInfo($"Election status with id {request.Id} updated successfully.");

            return Result<string>.Success("Election status updated successfully.");
        }

        public async Task<Result<IEnumerable<ElectionScopeResponse>>> GetElectionScopes()
        {
            _loggerMessage.LogInfo("Election scope list request received.");

            IQueryable<ElectionScope> query = _electionScopeRepo.GetQueryable().AsNoTracking().OrderBy(x => x.Name);

            List<ElectionScope> electionScopes = await query.ToListAsync();

            IEnumerable<ElectionScopeResponse> response = _mapper.Map<IEnumerable<ElectionScopeResponse>>(electionScopes);

            _loggerMessage.LogInfo($"{electionScopes.Count} election scopes found.");

            return Result<IEnumerable<ElectionScopeResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<ElectionScopeResponse>>> GetPagedElectionScopes(ElectionScopeRequest request)
        {
            _loggerMessage.LogInfo($"Paged election scope list request received for page {request.PageNumber}.");

            IQueryable<ElectionScope> query = _electionScopeRepo.GetQueryable().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();

                query = query.Where(electionScope => electionScope.Name.Contains(searchTerm)
                    || electionScope.Code.Contains(searchTerm));
            }

            query = query.OrderBy(x => x.Name);

            PagedList<ElectionScope> electionScopes = await query.GetPagedItems(request);

            PagedResponse<ElectionScopeResponse> response = _mapper.Map<PagedResponse<ElectionScopeResponse>>(electionScopes);

            _loggerMessage.LogInfo($"{electionScopes.MetaData.TotalCount} election scopes found.");

            return Result<PagedResponse<ElectionScopeResponse>>.Success(response);
        }

        public async Task<Result<ElectionScopeResponse>> GetElectionScope(int id)
        {
            _loggerMessage.LogInfo($"Election scope request received for id {id}.");

            ElectionScope? electionScope = await _electionScopeRepo.GetSingleByAsync(x => x.Id == id);
            if (electionScope is null)
            {
                _loggerMessage.LogWarn($"Election scope with id {id} was not found.");
                return Result<ElectionScopeResponse>.NotFound($"Election scope with id {id} was not found.");
            }

            ElectionScopeResponse response = _mapper.Map<ElectionScopeResponse>(electionScope);

            _loggerMessage.LogInfo($"Election scope with id {id} found successfully.");

            return Result<ElectionScopeResponse>.Success(response);
        }

        public async Task<Result<string>> UpdateElectionScope(UpdateElectionScopeRequest request)
        {
            _loggerMessage.LogInfo($"Election scope update request received for id {request.Id}.");

            ElectionScope? electionScope = await _electionScopeRepo.GetByIdAsync(request.Id);
            if (electionScope is null)
            {
                _loggerMessage.LogWarn($"Election scope update failed because election scope with id {request.Id} was not found.");
                return Result<string>.NotFound($"Election scope with id {request.Id} was not found.");
            }

            string electionScopeName = request.Name.Trim();

            bool electionScopeExists = await _electionScopeRepo.AnyAsync(x => x.Id != request.Id && x.Name == electionScopeName);
            if (electionScopeExists)
            {
                _loggerMessage.LogWarn($"Election scope update failed because election scope {electionScopeName} already exists.");
                return Result<string>.Conflict($"Election scope with name {electionScopeName} already exists.");
            }

            _mapper.Map(request, electionScope);

            await _electionScopeRepo.UpdateAsync(electionScope);

            _loggerMessage.LogInfo($"Election scope with id {request.Id} updated successfully.");

            return Result<string>.Success("Election scope updated successfully.");
        }

        public async Task<Result<string>> ToggleElectionScopeActivation(int id)
        {
            _loggerMessage.LogInfo($"Election scope toggle activation request received for id {id}.");

            ElectionScope? electionScope = await _electionScopeRepo.GetByIdAsync(id);
            if (electionScope is null)
            {
                _loggerMessage.LogWarn($"Election scope toggle activation failed because election scope with id {id} was not found.");
                return Result<string>.NotFound($"Election scope with id {id} was not found.");
            }

            electionScope.Active = !electionScope.Active;

            await _electionScopeRepo.UpdateAsync(electionScope);

            string status = electionScope.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Election scope with id {id} {status} successfully.");

            return Result<string>.Success($"Election scope {status} successfully.");
        }

        // Elections
        public async Task<Result<string>> CreateElection(CreateElectionRequest request)
        {
            _loggerMessage.LogInfo($"Election creation request received for {request.Name}.");

            Year? year = await _yearRepo.GetByIdAsync(request.YearId);
            if (year is null)
            {
                _loggerMessage.LogWarn($"Election creation failed because year with id {request.YearId} was not found.");
                return Result<string>.NotFound($"Year with id {request.YearId} was not found.");
            }

            ElectionType? electionType = await _electionTypeRepo.GetSingleByAsync(x => x.Id == request.ElectionTypeId,
                include: query => query.Include(x => x.ElectionScope));

            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election creation failed because election type with id {request.ElectionTypeId} was not found.");
                return Result<string>.NotFound($"Election type with id {request.ElectionTypeId} was not found.");
            }

            ElectionStatus? electionStatus = await _electionStatusRepo.GetByIdAsync(request.ElectionStatusId);
            if (electionStatus is null)
            {
                _loggerMessage.LogWarn($"Election creation failed because election status with id {request.ElectionStatusId} was not found.");
                return Result<string>.NotFound($"Election status with id {request.ElectionStatusId} was not found.");
            }

            if (electionStatus.Code != ApplicationConstants.ElectionStatusCodes.Draft && electionStatus.Code != ApplicationConstants.ElectionStatusCodes.RegistrationOpen)
            {
                _loggerMessage.LogWarn($"Election creation failed because status {electionStatus.Code} cannot be used as an initial election status.");
                return Result<string>.ValidationError("A new election can only be created with Draft or Registration Open status.");
            }

            if (electionStatus.Code == ApplicationConstants.ElectionStatusCodes.RegistrationOpen)
            {
                bool applicationPeriodProvided = !string.IsNullOrWhiteSpace(request.ApplicationStartAt)
                    && !string.IsNullOrWhiteSpace(request.ApplicationEndAt);

                if (!applicationPeriodProvided)
                {
                    _loggerMessage.LogWarn("Election creation failed because Registration Open requires an application period.");
                    return Result<string>.ValidationError("Registration Open requires ApplicationStartAt and ApplicationEndAt.");
                }
            }

            if (electionType.ElectionScope is null)
            {
                _loggerMessage.LogWarn($"Election creation failed because election type with id {request.ElectionTypeId} has no election scope.");
                return Result<string>.ValidationError("The selected election type does not have a valid election scope.");
            }

            bool electionExists;

            switch (electionType.ElectionScope.Code)
            {
                case ApplicationConstants.ElectionScopeCodes.University:
                    if (request.FacultyId.HasValue || request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("University election creation failed because a faculty or department was provided.");
                        return Result<string>.ValidationError("FacultyId and DepartmentId must not be provided for a university election.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.YearId == request.YearId && x.ElectionTypeId == request.ElectionTypeId
                        && x.FacultyId == null && x.DepartmentId == null);

                    break;

                case ApplicationConstants.ElectionScopeCodes.Faculty:
                    if (!request.FacultyId.HasValue)
                    {
                        _loggerMessage.LogWarn("Faculty election creation failed because no faculty was provided.");
                        return Result<string>.ValidationError("FacultyId is required for a faculty election.");
                    }

                    if (request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("Faculty election creation failed because a department was provided.");
                        return Result<string>.ValidationError("DepartmentId must not be provided for a faculty election.");
                    }

                    Faculty? faculty = await _facultyRepo.GetByIdAsync(request.FacultyId.Value);
                    if (faculty is null)
                    {
                        _loggerMessage.LogWarn($"Election creation failed because faculty with id {request.FacultyId.Value} was not found.");
                        return Result<string>.NotFound($"Faculty with id {request.FacultyId.Value} was not found.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.YearId == request.YearId && x.ElectionTypeId == request.ElectionTypeId
                        && x.FacultyId == request.FacultyId && x.DepartmentId == null);

                    break;

                case ApplicationConstants.ElectionScopeCodes.Department:
                    if (!request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("Department election creation failed because no department was provided.");
                        return Result<string>.ValidationError("DepartmentId is required for a department election.");
                    }

                    if (request.FacultyId.HasValue)
                    {
                        _loggerMessage.LogWarn("Department election creation failed because a faculty was provided.");
                        return Result<string>.ValidationError("FacultyId must not be provided for a department election.");
                    }

                    Department? department = await _departmentRepo.GetByIdAsync(request.DepartmentId.Value);
                    if (department is null)
                    {
                        _loggerMessage.LogWarn($"Election creation failed because department with id {request.DepartmentId.Value} was not found.");
                        return Result<string>.NotFound($"Department with id {request.DepartmentId.Value} was not found.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.YearId == request.YearId && x.ElectionTypeId == request.ElectionTypeId
                        && x.DepartmentId == request.DepartmentId && x.FacultyId == null);

                    break;

                default:
                    _loggerMessage.LogWarn($"Election creation failed because election scope code {electionType.ElectionScope.Code} is not supported.");
                    return Result<string>.ValidationError($"Election scope {electionType.ElectionScope.Code} is not supported.");
            }

            if (electionExists)
            {
                _loggerMessage.LogWarn($"Election creation failed because an election already exists for election type {request.ElectionTypeId} and year {request.YearId}.");
                return Result<string>.Conflict("An election already exists for the selected election type, year and scope.");
            }

            Election election = _mapper.Map<Election>(request);

            await _electionRepo.AddAsync(election);

            _loggerMessage.LogInfo($"Election {election.Name} created successfully.");

            return Result<string>.Created($"Election with name {election.Name} created successfully.");
        }

        public async Task<Result<IEnumerable<ElectionResponse>>> GetElections(ElectionRequest request)
        {
            _loggerMessage.LogInfo("Election list request received.");

            IQueryable<Election> query = GetFilteredElectionQuery(request);

            List<Election> elections = await query.ToListAsync();

            IEnumerable<ElectionResponse> response = _mapper.Map<IEnumerable<ElectionResponse>>(elections);

            _loggerMessage.LogInfo($"{elections.Count} elections found.");

            return Result<IEnumerable<ElectionResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<ElectionResponse>>> GetPagedElections(ElectionRequest request)
        {
            _loggerMessage.LogInfo($"Paginated election list request received for page {request.PageNumber}.");

            IQueryable<Election> query = GetFilteredElectionQuery(request);

            PagedList<Election> elections = await query.GetPagedItems(request);

            PagedResponse<ElectionResponse> response = _mapper.Map<PagedResponse<ElectionResponse>>(elections);

            _loggerMessage.LogInfo($"{elections.MetaData.TotalCount} elections found.");

            return Result<PagedResponse<ElectionResponse>>.Success(response);
        }

        public async Task<Result<ElectionResponse>> GetElection(string id)
        {
            _loggerMessage.LogInfo($"Election request received for id {id}.");

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == id, include: query => query
                .Include(x => x.Year).Include(x => x.ElectionType).Include(x => x.ElectionStatus).Include(x => x.Faculty)
                .Include(x => x.Department).ThenInclude(x => x.Faculty).Include(x => x.ElectionPositions));

            if (election is null)
            {
                _loggerMessage.LogWarn($"Election with id {id} was not found.");
                return Result<ElectionResponse>.NotFound($"Election with id {id} was not found.");
            }

            ElectionResponse response = _mapper.Map<ElectionResponse>(election);

            _loggerMessage.LogInfo($"Election {election.Name} found.");

            return Result<ElectionResponse>.Success(response);
        }

        public async Task<Result<string>> UpdateElection(CreateElectionRequest request)
        {
            _loggerMessage.LogInfo($"Election update request received for id {request.Id}.");

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                _loggerMessage.LogWarn("Election update failed because election id was not provided.");
                return Result<string>.ValidationError("Election id is required.");
            }

            Election? election = await _electionRepo.GetSingleByAsync(x => x.Id == request.Id, include: query => query.Include(x => x.ElectionStatus));
            if (election is null)
            {
                _loggerMessage.LogWarn($"Election update failed because election with id {request.Id} was not found.");
                return Result<string>.NotFound($"Election with id {request.Id} was not found.");
            }

            Year? year = await _yearRepo.GetByIdAsync(request.YearId);
            if (year is null)
            {
                _loggerMessage.LogWarn($"Election update failed because year with id {request.YearId} was not found.");
                return Result<string>.NotFound($"Year with id {request.YearId} was not found.");
            }

            ElectionType? electionType = await _electionTypeRepo.GetSingleByAsync(x => x.Id == request.ElectionTypeId, include: query => query.Include(x => x.ElectionScope));
            if (electionType is null)
            {
                _loggerMessage.LogWarn($"Election update failed because election type with id {request.ElectionTypeId} was not found.");
                return Result<string>.NotFound($"Election type with id {request.ElectionTypeId} was not found.");
            }

            ElectionStatus currentElectionStatus = election.ElectionStatus;

            ElectionStatus? requestedElectionStatus;

            if (request.ElectionStatusId == election.ElectionStatusId)
            {
                requestedElectionStatus = currentElectionStatus;
            }
            else
            {
                requestedElectionStatus = await _electionStatusRepo.GetByIdAsync(request.ElectionStatusId);
            }

            if (requestedElectionStatus is null)
            {
                _loggerMessage.LogWarn($"Election update failed because election status with id {request.ElectionStatusId} was not found.");
                return Result<string>.NotFound($"Election status with id {request.ElectionStatusId} was not found.");
            }

            bool validStatusTransition = IsValidElectionStatusTransition(currentElectionStatus.Code, requestedElectionStatus.Code);
            if (!validStatusTransition)
            {
                _loggerMessage.LogWarn(
                    $"Election update failed because status transition from {currentElectionStatus.Code} to {requestedElectionStatus.Code} is invalid.");

                return Result<string>.ValidationError($"Election status cannot change from {currentElectionStatus.Name} to {requestedElectionStatus.Name}.");
            }

            string? statusValidationError = GetElectionStatusValidationError(requestedElectionStatus.Code, request);
            if (statusValidationError is not null)
            {
                _loggerMessage.LogWarn($"Election update failed because {statusValidationError}");
                return Result<string>.ValidationError(statusValidationError);
            }

            if (electionType.ElectionScope is null)
            {
                _loggerMessage.LogWarn($"Election update failed because election type with id {request.ElectionTypeId} has no election scope.");
                return Result<string>.ValidationError("Election type does not have a valid election scope.");
            }

            bool electionExists;

            switch (electionType.ElectionScope.Code)
            {
                case ApplicationConstants.ElectionScopeCodes.University:
                    if (request.FacultyId.HasValue || request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("Election update failed because a university election cannot have a faculty or department.");
                        return Result<string>.ValidationError("University elections cannot have FacultyId or DepartmentId.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.Id != request.Id && x.YearId == request.YearId
                        && x.ElectionTypeId == request.ElectionTypeId && x.FacultyId == null && x.DepartmentId == null);

                    break;

                case ApplicationConstants.ElectionScopeCodes.Faculty:
                    if (!request.FacultyId.HasValue)
                    {
                        _loggerMessage.LogWarn("Election update failed because a faculty election requires a faculty.");
                        return Result<string>.ValidationError("Faculty elections require FacultyId.");
                    }

                    if (request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("Election update failed because a faculty election cannot have a department.");
                        return Result<string>.ValidationError("Faculty elections cannot have DepartmentId.");
                    }

                    Faculty? faculty = await _facultyRepo.GetByIdAsync(request.FacultyId.Value);
                    if (faculty is null)
                    {
                        _loggerMessage.LogWarn($"Election update failed because faculty with id {request.FacultyId.Value} was not found.");
                        return Result<string>.NotFound($"Faculty with id {request.FacultyId.Value} was not found.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.Id != request.Id && x.YearId == request.YearId
                        && x.ElectionTypeId == request.ElectionTypeId && x.FacultyId == request.FacultyId.Value);

                    break;

                case ApplicationConstants.ElectionScopeCodes.Department:
                    if (!request.DepartmentId.HasValue)
                    {
                        _loggerMessage.LogWarn("Election update failed because a department election requires a department.");
                        return Result<string>.ValidationError("Department elections require DepartmentId.");
                    }

                    if (request.FacultyId.HasValue)
                    {
                        _loggerMessage.LogWarn("Election update failed because a department election cannot have a faculty.");
                        return Result<string>.ValidationError("Department elections cannot have FacultyId.");
                    }

                    Department? department = await _departmentRepo.GetByIdAsync(request.DepartmentId.Value);
                    if (department is null)
                    {
                        _loggerMessage.LogWarn($"Election update failed because department with id {request.DepartmentId.Value} was not found.");
                        return Result<string>.NotFound($"Department with id {request.DepartmentId.Value} was not found.");
                    }

                    electionExists = await _electionRepo.AnyAsync(x => x.Id != request.Id && x.YearId == request.YearId
                        && x.ElectionTypeId == request.ElectionTypeId && x.DepartmentId == request.DepartmentId.Value);

                    break;

                default:
                    _loggerMessage.LogWarn($"Election update failed because election scope {electionType.ElectionScope.Code} is unsupported.");
                    return Result<string>.ValidationError($"Election scope {electionType.ElectionScope.Code} is not supported.");
            }

            if (electionExists)
            {
                _loggerMessage.LogWarn("Election update failed because another election already exists for the selected year, election type and scope.");
                return Result<string>.Conflict("An election already exists for the selected election type, year and scope.");
            }

            _mapper.Map(request, election);

            election.ElectionStatus = requestedElectionStatus;

            await _electionRepo.UpdateAsync(election);

            _loggerMessage.LogInfo($"Election with id {request.Id} updated successfully.");

            return Result<string>.Success("Election updated successfully.");
        }

        public async Task<Result<string>> ToggleElectionActivation(string id)
        {
            _loggerMessage.LogInfo($"Election toggle activation request received for id {id}.");

            Election? election = await _electionRepo.GetByIdAsync(id);
            if (election is null)
            {
                _loggerMessage.LogWarn($"Election toggle activation failed because election with id {id} was not found.");
                return Result<string>.NotFound($"Election with id {id} was not found.");
            }

            election.Active = !election.Active;

            await _electionRepo.UpdateAsync(election);

            string status = election.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Election with id {id} {status} successfully.");

            return Result<string>.Success($"Election {status} successfully.");
        }

        private IQueryable<Election> GetFilteredElectionQuery(ElectionRequest request)
        {
            IQueryable<Election> query = _electionRepo.GetQueryable(include: query => query.Include(x => x.Year).Include(x => x.ElectionType)
                .Include(x => x.ElectionStatus).Include(x => x.Faculty).Include(x => x.ElectionPositions)
                .Include(x => x.Department).ThenInclude(x => x.Faculty))
            .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();

                query = query.Where(x => x.Name.Contains(searchTerm)
                    || x.Year.Name.Contains(searchTerm)
                    || x.ElectionType.Name.Contains(searchTerm)
                    || x.ElectionStatus.Name.Contains(searchTerm)
                    || (x.Faculty != null && x.Faculty.Name.Contains(searchTerm))
                    || (x.Department != null && x.Department.Name.Contains(searchTerm)));
            }

            if (request.YearId.HasValue)
                query = query.Where(x => x.YearId == request.YearId.Value);

            if (request.ElectionTypeId.HasValue)
                query = query.Where(x => x.ElectionTypeId == request.ElectionTypeId.Value);

            if (request.ElectionStatusId.HasValue)
                query = query.Where(x => x.ElectionStatusId == request.ElectionStatusId.Value);

            if (request.FacultyId.HasValue)
                query = query.Where(x => x.FacultyId == request.FacultyId.Value
                    || (x.Department != null && x.Department.FacultyId == request.FacultyId.Value));

            if (request.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

            if (request.Active.HasValue)
                query = query.Where(x => x.Active == request.Active.Value);

            return query.OrderBy(x => x.Name);
        }

        private static bool IsValidElectionStatusTransition(string currentStatusCode, string targetStatusCode)
        {
            if (currentStatusCode == targetStatusCode)
                return true;

            return currentStatusCode switch
            {
                ApplicationConstants.ElectionStatusCodes.Draft => targetStatusCode == ApplicationConstants.ElectionStatusCodes.RegistrationOpen
                    || targetStatusCode == ApplicationConstants.ElectionStatusCodes.Cancelled,

                ApplicationConstants.ElectionStatusCodes.RegistrationOpen => targetStatusCode == ApplicationConstants.ElectionStatusCodes.RegistrationClosed
                    || targetStatusCode == ApplicationConstants.ElectionStatusCodes.Cancelled,

                ApplicationConstants.ElectionStatusCodes.RegistrationClosed => targetStatusCode == ApplicationConstants.ElectionStatusCodes.VotingOpen
                    || targetStatusCode == ApplicationConstants.ElectionStatusCodes.Cancelled,

                ApplicationConstants.ElectionStatusCodes.VotingOpen => targetStatusCode == ApplicationConstants.ElectionStatusCodes.Completed
                    || targetStatusCode == ApplicationConstants.ElectionStatusCodes.Cancelled,

                ApplicationConstants.ElectionStatusCodes.Completed => false,

                ApplicationConstants.ElectionStatusCodes.Cancelled => false,

                _ => false
            };
        }

        private static string? GetElectionStatusValidationError(string statusCode, CreateElectionRequest request)
        {
            bool applicationPeriodProvided = !string.IsNullOrWhiteSpace(request.ApplicationStartAt)
                && !string.IsNullOrWhiteSpace(request.ApplicationEndAt);

            bool voterRegistrationPeriodProvided = !string.IsNullOrWhiteSpace(request.VoterRegistrationStartAt)
                && !string.IsNullOrWhiteSpace(request.VoterRegistrationEndAt);

            bool votingPeriodProvided = !string.IsNullOrWhiteSpace(request.VotingStartAt)
                && !string.IsNullOrWhiteSpace(request.VotingEndAt);

            if (statusCode == ApplicationConstants.ElectionStatusCodes.RegistrationOpen && !applicationPeriodProvided)
                return "Registration Open requires ApplicationStartAt and ApplicationEndAt.";

            if (statusCode == ApplicationConstants.ElectionStatusCodes.RegistrationClosed && (!applicationPeriodProvided || !voterRegistrationPeriodProvided))
                return "Registration Closed requires the application and voter registration periods.";

            if (statusCode == ApplicationConstants.ElectionStatusCodes.VotingOpen && !votingPeriodProvided)
                return "Voting Open requires VotingStartAt and VotingEndAt.";

            if (statusCode == ApplicationConstants.ElectionStatusCodes.Completed && !votingPeriodProvided)
                return "Completed requires VotingStartAt and VotingEndAt.";

            return null;
        }
    }
}
