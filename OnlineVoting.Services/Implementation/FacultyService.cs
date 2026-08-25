using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class FacultyService : IFacultyService
    {
        private readonly IRepository<Faculty> _facultyRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public FacultyService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _facultyRepo = _unitOfWork.GetRepository<Faculty>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateFaculty(CreateFacultyRequest request)
        {
            _loggerMessage.LogInfo("Faculty creation request received.");

            bool facultyName = !string.IsNullOrWhiteSpace(request.Name);
            bool facultyNames = request.Names is { Count: > 0 };

            if (!facultyName && !facultyNames)
            {
                _loggerMessage.LogWarn("Faculty creation failed because no faculty name was provided.");

                return Result<string>.ValidationError("Provide either a faculty name or a list of faculty names.");
            }

            if (facultyName && facultyNames)
            {
                _loggerMessage.LogWarn("Faculty creation failed because both Name and Names were provided.");

                return Result<string>.ValidationError("Provide either Name or Names, but not both.");
            }

            if (facultyName)
            {
                string facultyNameToAdd = request.Name!.Trim();

                Faculty? existingFaculty = await _facultyRepo.GetSingleByAsync(faculty => faculty.Name == facultyNameToAdd);

                if (existingFaculty is not null)
                {
                    _loggerMessage.LogWarn($"Faculty creation skipped because faculty {facultyNameToAdd} already exists.");

                    return Result<string>.Conflict($"Faculty with name {facultyNameToAdd} already exists.");
                }

                Faculty faculty = _mapper.Map<Faculty>(request);
                faculty.Name = facultyNameToAdd;

                await _facultyRepo.AddAsync(faculty);

                _loggerMessage.LogInfo($"Faculty {faculty.Name} created successfully.");

                return Result<string>.Created($"Faculty with name {faculty.Name} created successfully");
            }

            List<string> facultyNamesToAdd = request.Names!
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (facultyNamesToAdd.Count == 0)
            {
                _loggerMessage.LogWarn("Faculty creation failed because the faculty name list was empty.");

                return Result<string>.ValidationError("Faculty names cannot be empty.");
            }

            List<string> existingFacultyNames = await _facultyRepo.SelectAsync(faculty => facultyNamesToAdd.Contains(faculty.Name), faculty => faculty.Name);

            HashSet<string> existingNames = existingFacultyNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<string> newFacultyNames = facultyNamesToAdd.Where(name => !existingNames.Contains(name)).ToList();

            if (newFacultyNames.Count == 0)
            {
                _loggerMessage.LogInfo("Faculty creation completed with no new faculties because all requested faculties already exist.");

                return Result<string>.Success("No new faculties were created.");
            }

            List<Faculty> faculties = _mapper.Map<List<Faculty>>(newFacultyNames);

            await _facultyRepo.AddRangeAsync(faculties);

            _loggerMessage.LogInfo($"{faculties.Count} faculties created successfully.");

            return Result<string>.Created($"{faculties.Count} faculties created successfully.");
        }

        public async Task<Result<FacultyResponse>> GetFaculty(long id)
        {
            _loggerMessage.LogInfo($"Faculty request received for id {id}.");

            Faculty? faculty = await _facultyRepo.GetByIdAsync(id);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Faculty with id {id} was not found.");

                return Result<FacultyResponse>.NotFound($"Faculty with id {id} was not found.");
            }

            FacultyResponse response = _mapper.Map<FacultyResponse>(faculty);

            return Result<FacultyResponse>.Success(response);
        }

        public async Task<Result<string>> UpdateFaculty(long id, CreateWithNameRequest request)
        {
            _loggerMessage.LogInfo($"Faculty update request received for id {id}.");

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _loggerMessage.LogWarn($"Faculty update failed for id {id} because the faculty name was empty.");

                return Result<string>.ValidationError("Faculty name cannot be empty.");
            }

            Faculty? faculty = await _facultyRepo.GetByIdAsync(id);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Faculty update failed because faculty with id {id} was not found.");

                return Result<string>.NotFound($"Faculty with id {id} was not found.");
            }

            string facultyName = request.Name.Trim();

            bool facultyExists = await _facultyRepo.AnyAsync(item => item.Id != id && item.Name == facultyName);

            if (facultyExists)
            {
                _loggerMessage.LogWarn($"Faculty update failed because faculty {facultyName} already exists.");

                return Result<string>.Conflict($"Faculty with name {facultyName} already exists.");
            }

            faculty.Name = facultyName;

            await _facultyRepo.UpdateAsync(faculty);

            _loggerMessage.LogInfo($"Faculty with id {id} updated successfully.");

            return Result<string>.Success("Faculty updated successfully.");
        }

        public async Task<Result<string>> ToggleFacultyActivation(long id)
        {
            _loggerMessage.LogInfo($"Faculty activation update request received for id {id}.");

            Faculty? faculty = await _facultyRepo.GetByIdAsync(id);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Faculty activation update failed because faculty with id {id} was not found.");

                return Result<string>.NotFound($"Faculty with id {id} was not found.");
            }           

            faculty.Active = !faculty.Active;

            await _facultyRepo.UpdateAsync(faculty);

            string status = faculty.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Faculty with id {id} {status} successfully.");

            return Result<string>.Success($"Faculty {status} successfully.");
        }

        public async Task<Result<string>> DeleteFaculty(long id)
        {
            _loggerMessage.LogInfo($"Faculty deletion request received for id {id}.");

            Faculty? faculty = await _facultyRepo.GetByIdAsync(id);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Faculty deletion failed because faculty with id {id} was not found.");

                return Result<string>.NotFound($"Faculty with id {id} was not found.");
            }

            await _facultyRepo.DeleteAsync(faculty);

            _loggerMessage.LogInfo($"Faculty with id {id} deleted successfully.");

            return Result<string>.Success("Faculty deleted successfully.");
        }

        public async Task<Result<PagedResponse<FacultyResponse>>> GetFaculties(FacultyRequestParameters request)
        {
            _loggerMessage.LogInfo($"Faculty list request received for page {request.PageNumber}.");

            PagedList<Faculty> faculties = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _facultyRepo.GetPagedItems(request)
                : await _facultyRepo.GetPagedItems(request, faculty => faculty.Name.Contains(request.SearchTerm.Trim()));

            PagedResponse<FacultyResponse> response = _mapper.Map<PagedResponse<FacultyResponse>>(faculties);

            _loggerMessage.LogInfo($"{faculties.MetaData.TotalCount} faculties found.");

            return Result<PagedResponse<FacultyResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<FacultyResponse>>> GetFacultiesWithDepartments(FacultyRequestParameters request)
        {
            _loggerMessage.LogInfo($"Faculty with departments list request received for page {request.PageNumber}.");

            PagedList<Faculty> faculties = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _facultyRepo.GetPagedItems(request, include: query => query.Include(faculty => faculty.Departments))
                : await _facultyRepo.GetPagedItems(request, faculty => faculty.Name.Contains(request.SearchTerm.Trim()),
                    include: query => query.Include(faculty => faculty.Departments));

            PagedResponse<FacultyResponse> response = _mapper.Map<PagedResponse<FacultyResponse>>(faculties);

            _loggerMessage.LogInfo($"{faculties.MetaData.TotalCount} faculties with departments found.");

            return Result<PagedResponse<FacultyResponse>>.Success(response);
        }

        public async Task<Result<FacultyResponse>> GetFacultyWithDepartments(long id)
        {
            _loggerMessage.LogInfo($"Faculty with departments request received for id {id}.");

            Faculty? faculty = await _facultyRepo.GetSingleByAsync(item => item.Id == id,
                include: query => query.Include(item => item.Departments));

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Faculty with id {id} was not found.");

                return Result<FacultyResponse>.NotFound($"Faculty with id {id} was not found.");
            }

            FacultyResponse response = _mapper.Map<FacultyResponse>(faculty);

            _loggerMessage.LogInfo($"Faculty with id {id} found with {faculty.Departments.Count} departments.");

            return Result<FacultyResponse>.Success(response);
        }
    }
}