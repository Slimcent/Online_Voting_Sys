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
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _deptRepo;
        private readonly IRepository<Faculty> _facultyRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public DepartmentService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _deptRepo = _unitOfWork.GetRepository<Department>();
            _facultyRepo = _unitOfWork.GetRepository<Faculty>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateDepartment(CreateDepartmentRequest request)
        {
            _loggerMessage.LogInfo($"Department creation request received for faculty {request.FacultyId}.");

            bool departmentName = !string.IsNullOrWhiteSpace(request.Name);
            bool departmentNames = request.Names is { Count: > 0 };

            if (!departmentName && !departmentNames)
            {
                _loggerMessage.LogWarn("Department creation failed because no department name was provided.");

                return Result<string>.ValidationError("Provide either a department name or a list of department names.");
            }

            if (departmentName && departmentNames)
            {
                _loggerMessage.LogWarn("Department creation failed because both Name and Names were provided.");

                return Result<string>.ValidationError("Provide either Name or Names, but not both.");
            }

            Faculty? faculty = await _facultyRepo.GetByIdAsync(request.FacultyId);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Department creation failed because faculty with id {request.FacultyId} was not found.");

                return Result<string>.NotFound($"Faculty with id {request.FacultyId} was not found.");
            }

            if (departmentName)
            {
                string departmentNameToAdd = request.Name!.Trim();

                Department? existingDepartment = await _deptRepo.GetSingleByAsync(department => department.Name == departmentNameToAdd);

                if (existingDepartment is not null)
                {
                    _loggerMessage.LogWarn($"Department creation skipped because department {departmentNameToAdd} already exists.");

                    return Result<string>.Conflict($"Department with name {departmentNameToAdd} already exists.");
                }

                Department department = _mapper.Map<Department>(request);
                department.Name = departmentNameToAdd;

                await _deptRepo.AddAsync(department);

                _loggerMessage.LogInfo($"Department {department.Name} created successfully.");

                return Result<string>.Created($"Department with name {department.Name} created successfully");
            }

            List<string> departmentNamesToAdd = request.Names!
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (departmentNamesToAdd.Count == 0)
            {
                _loggerMessage.LogWarn("Department creation failed because the department name list was empty.");

                return Result<string>.ValidationError("Department names cannot be empty.");
            }

            List<string> existingDepartmentNames = await _deptRepo.SelectAsync(department => departmentNamesToAdd.Contains(department.Name), department => department.Name);

            HashSet<string> existingNames = existingDepartmentNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<string> newDepartmentNames = departmentNamesToAdd.Where(name => !existingNames.Contains(name)).ToList();

            if (newDepartmentNames.Count == 0)
            {
                _loggerMessage.LogInfo("Department creation completed with no new departments because all requested departments already exist.");

                return Result<string>.Success("No new departments were created.");
            }

            List<Department> departments = _mapper.Map<List<Department>>(newDepartmentNames);

            foreach (Department department in departments)
                department.FacultyId = request.FacultyId;

            await _deptRepo.AddRangeAsync(departments);

            _loggerMessage.LogInfo($"{departments.Count} departments created successfully.");

            return Result<string>.Created($"{departments.Count} departments created successfully.");
        }

        public async Task<Result<PagedResponse<DepartmentResponse>>> GetDepartments(DepartmentRequestParameters request)
        {
            _loggerMessage.LogInfo($"Department list request received for page {request.PageNumber}.");

            PagedList<Department> departments = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _deptRepo.GetPagedItems(request, include: query => query.Include(department => department.Faculty))
                : await _deptRepo.GetPagedItems(request, department => department.Name.Contains(request.SearchTerm.Trim()),
                    include: query => query.Include(department => department.Faculty));

            PagedResponse<DepartmentResponse> response = _mapper.Map<PagedResponse<DepartmentResponse>>(departments);

            _loggerMessage.LogInfo($"{departments.MetaData.TotalCount} departments found.");

            return Result<PagedResponse<DepartmentResponse>>.Success(response);
        }

        public async Task<Result<DepartmentResponse>> GetDepartment(long id)
        {
            _loggerMessage.LogInfo($"Department request received for id {id}.");

            Department? department = await _deptRepo.GetSingleByAsync(x => x.Id == id,
                include: query => query.Include(x => x.Faculty));

            if (department is null)
            {
                _loggerMessage.LogWarn($"Department with id {id} was not found.");

                return Result<DepartmentResponse>.NotFound($"Department with id {id} was not found.");
            }

            DepartmentResponse response = _mapper.Map<DepartmentResponse>(department);

            return Result<DepartmentResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<DepartmentResponse>>> GetDepartmentsByFacultyId(long facultyId)
        {
            _loggerMessage.LogInfo($"Department list request received for faculty id {facultyId}.");

            Faculty? faculty = await _facultyRepo.GetByIdAsync(facultyId);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Department list request failed because faculty with id {facultyId} was not found.");

                return Result<IEnumerable<DepartmentResponse>>.NotFound($"Faculty with id {facultyId} was not found.");
            }

            IEnumerable<Department> departments = await _deptRepo.GetByAsync(department => department.FacultyId == facultyId,
                orderBy: query => query.OrderBy(department => department.Name),
                include: query => query.Include(department => department.Faculty));

            IEnumerable<DepartmentResponse> response = _mapper.Map<IEnumerable<DepartmentResponse>>(departments);

            _loggerMessage.LogInfo($"{response.Count()} departments found for faculty id {facultyId}.");

            return Result<IEnumerable<DepartmentResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<DepartmentResponse>>> GetDepartmentsByFacultyId(long facultyId, DepartmentRequestParameters request)
        {
            _loggerMessage.LogInfo($"Paginated department list request received for faculty id {facultyId} and page {request.PageNumber}.");

            Faculty? faculty = await _facultyRepo.GetByIdAsync(facultyId);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Paginated department list request failed because faculty with id {facultyId} was not found.");

                return Result<PagedResponse<DepartmentResponse>>.NotFound($"Faculty with id {facultyId} was not found.");
            }

            PagedList<Department> departments = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _deptRepo.GetPagedItems(request, department => department.FacultyId == facultyId,
                    include: query => query.Include(department => department.Faculty))
                : await _deptRepo.GetPagedItems(request, department => department.FacultyId == facultyId &&
                    department.Name.Contains(request.SearchTerm.Trim()),
                    include: query => query.Include(department => department.Faculty));

            PagedResponse<DepartmentResponse> response = _mapper.Map<PagedResponse<DepartmentResponse>>(departments);

            _loggerMessage.LogInfo($"{departments.MetaData.TotalCount} departments found for faculty id {facultyId}.");

            return Result<PagedResponse<DepartmentResponse>>.Success(response);
        }

        public async Task<Result<string>> UpdateDepartment(long id, CreateDepartmentRequest request)
        {
            _loggerMessage.LogInfo($"Department update request received for id {id}.");

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _loggerMessage.LogWarn($"Department update failed for id {id} because the department name was empty.");

                return Result<string>.ValidationError("Department name cannot be empty.");
            }

            Department? department = await _deptRepo.GetByIdAsync(id);

            if (department is null)
            {
                _loggerMessage.LogWarn($"Department update failed because department with id {id} was not found.");

                return Result<string>.NotFound($"Department with id {id} was not found.");
            }

            Faculty? faculty = await _facultyRepo.GetByIdAsync(request.FacultyId);

            if (faculty is null)
            {
                _loggerMessage.LogWarn($"Department update failed because faculty with id {request.FacultyId} was not found.");

                return Result<string>.NotFound($"Faculty with id {request.FacultyId} was not found.");
            }

            string departmentName = request.Name.Trim();

            bool departmentExists = await _deptRepo.AnyAsync(x => x.Id != id && x.Name == departmentName);

            if (departmentExists)
            {
                _loggerMessage.LogWarn($"Department update failed because department {departmentName} already exists.");

                return Result<string>.Conflict($"Department with name {departmentName} already exists.");
            }

            department.Name = departmentName;
            department.FacultyId = request.FacultyId;

            await _deptRepo.UpdateAsync(department);

            _loggerMessage.LogInfo($"Department with id {id} updated successfully.");

            return Result<string>.Success("Department updated successfully.");
        }

        public async Task<Result<string>> ToggleDepartmentActivation(long id)
        {
            _loggerMessage.LogInfo($"Department toggle activation request received for id {id}.");

            Department? department = await _deptRepo.GetByIdAsync(id);

            if (department is null)
            {
                _loggerMessage.LogWarn($"Department toggle activation update failed because department with id {id} was not found.");

                return Result<string>.NotFound($"Department with id {id} was not found.");
            }
                        
            department.Active = !department.Active;

            await _deptRepo.UpdateAsync(department);

            string status = department.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Department with id {id} {status} successfully.");

            return Result<string>.Success($"Department {status} successfully.");
        }

        public async Task<Result<string>> DeleteDepartment(long id)
        {
            _loggerMessage.LogInfo($"Department deletion request received for id {id}.");

            Department? department = await _deptRepo.GetByIdAsync(id);

            if (department is null)
            {
                _loggerMessage.LogWarn($"Department deletion failed because department with id {id} was not found.");

                return Result<string>.NotFound($"Department with id {id} was not found.");
            }

            await _deptRepo.DeleteAsync(department);

            _loggerMessage.LogInfo($"Department with id {id} deleted successfully.");

            return Result<string>.Success("Department deleted successfully.");
        }
    }
}