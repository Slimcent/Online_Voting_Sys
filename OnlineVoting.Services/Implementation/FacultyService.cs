using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Data.Interfaces;

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

        public async Task<Result<string>> CreateFaculty(CreateWithNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Faculty name cannot be empty.");

            Faculty? facultyExists = await _facultyRepo.GetSingleByAsync(faculty => faculty.Name == request.Name);

            if (facultyExists is not null)
                return Result<string>.Conflict("Faculty already exists.");

            Faculty addFaculty = _mapper.Map<Faculty>(request);

            addFaculty.Activated = true;

            await _facultyRepo.AddAsync(addFaculty);

            string message = $"Faculty with name {addFaculty.Name} created successfully";

            return Result<string>.Created(message);
        }
    }
}