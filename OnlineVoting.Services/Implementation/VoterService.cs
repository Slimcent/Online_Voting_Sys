using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class VoterService : IVoterService
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<RegisteredVoter> _registeredVoterRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public VoterService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
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

            _registeredVoterRepo.Add(registerVoter);

            VoterEmailDto emailDto = new()
            {
                Email = checkIfStudentExists.User.Email,
                VotingCode = votingCode,
                FirstName = checkIfStudentExists.User.FirstName,
            };

            await _serviceFactory.GetService<IEmailService>().SendVoterEmail(emailDto);

            await _unitOfWork.SaveChangesAsync();

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