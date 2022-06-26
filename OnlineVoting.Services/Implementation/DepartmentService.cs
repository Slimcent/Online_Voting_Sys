using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<string> CreateDepartment(DeptCreateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name cannot be empty");

            Department deptExists = await _deptRepo.GetSingleByAsync(x => x.Name == request.Name);
            if (deptExists != null)
                throw new InvalidOperationException("Faculty already exists");

            Department addDepartment = _mapper.Map<Department>(request);
            addDepartment.Activated = true;

            await _deptRepo.AddAsync(addDepartment);

            return $"Faculty with name {addDepartment.Name} created successfully";
        }
    }
}
