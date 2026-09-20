using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;


namespace OnlineVoting.Services.Implementation
{
    public class VoterService : IVoterService
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<RegisteredVoter> _registeredVoterRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public VoterService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<string> CreateVoter(CreateVoterRequest request)
        {
            Student checkIfStudentExists = await _studentRepo.GetSingleByAsync(x => x.RegNumber == request.RegNumber,
                include: x => x.Include(d => d.Department).ThenInclude(f => f.Faculty).Include(x => x.User));

            if (checkIfStudentExists == null)
                throw new NotFoundException(request.RegNumber);

            RegisteredVoter checkIfStudentAlreadyRegistered = await _registeredVoterRepo.GetSingleByAsync(x => x.StudentId == checkIfStudentExists.Id
                && x.DepartmentId == checkIfStudentExists.DepartmentId);

            if (checkIfStudentAlreadyRegistered != null)
                throw new InvalidOperationException($"Student with regNo {checkIfStudentExists.RegNumber} has already registered to vote");

            string votingCode = VotingCodeExtention.StudentVotingCode();

            RegisteredVoter registerVoter = new()
            {
                StudentId = checkIfStudentExists.Id,
                VotingCode = votingCode,
                DepartmentId = checkIfStudentExists.DepartmentId,
            };

            await _registeredVoterRepo.AddAsync(registerVoter);

            VoterEmailDto emailDto = new()
            {
                Email = checkIfStudentExists.User.Email,
                VotingCode = votingCode,
                FirstName = checkIfStudentExists.User.FirstName,
            };

            try
            {
                _serviceFactory.GetService<IBackgroundTaskQueue>().Enqueue<SendVoterEmailTask, VoterEmailDto>(emailDto);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Voter email could not be queued for student {checkIfStudentExists.Id}. {exception.Message}");
            }

            return "Voter registration was successful";
        }

        public async Task<string> ToggleVoter(Guid id)
        {
            RegisteredVoter registeredVoter = await _registeredVoterRepo.GetSingleByAsync(x => x.Id == id);

            if (registeredVoter == null)
                throw new InvalidOperationException("Student not found");

            registeredVoter.IsDeActivated = !registeredVoter.IsDeActivated;

            if (registeredVoter.IsDeActivated == true)
            {
                return "Voter Deactivated successfully";
            }
            else
            {
                return "Voter activated successfully";
            }
        }
    }
}