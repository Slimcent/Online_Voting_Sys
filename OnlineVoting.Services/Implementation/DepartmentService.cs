using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _deptRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _deptRepo = _unitOfWork.GetRepository<Department>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Result<string>> CreateDepartment(CreateDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Department name cannot be empty.");

            Department? deptExists = await _deptRepo.GetSingleByAsync(department => department.Name == request.Name);

            if (deptExists is not null)
                return Result<string>.Conflict("Department already exists.");

            Department addDepartment = _mapper.Map<Department>(request);

            addDepartment.Activated = true;

            await _deptRepo.AddAsync(addDepartment);

            string message = $"Department with name {addDepartment.Name} created successfully";

            return Result<string>.Created(message);
        }
    }
}