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
    public class FacultyService : IFacultyService
    {
        private readonly IRepository<Faculty> _facultyRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public FacultyService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _facultyRepo = _unitOfWork.GetRepository<Faculty>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public  async Task<string> CreateFaculty(CreateFacultyDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name cannot be empty");

            Faculty facultyExists = await _facultyRepo.GetSingleByAsync(x => x.Name == request.Name);
            if (facultyExists != null)
                throw new InvalidOperationException("Faculty already exists");

            Faculty addPosition = _mapper.Map<Faculty>(request);

            await _facultyRepo.AddAsync(addPosition);

            return $"Faculty with name {addPosition.Name} created successfully";
        }
    }
}
