using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<string> CreateVoter(VoterCreateDto request)
        {
            Student checkIfStudentExists = await _studentRepo.GetSingleByAsync(x => x.RegNo == request.RegNo, 
                include: x => x.Include(d => d.Department).ThenInclude(f => f.Faculty));

            if (checkIfStudentExists == null)
                throw new StudentNotFoundException(request.RegNo);

            RegisteredVoter checkIfStudentAlreadyRegistered = await _registeredVoterRepo.GetSingleByAsync(x => x.StudentId == checkIfStudentExists.Id 
                && x.DepartmentId == checkIfStudentExists.DepartmentId);

            if (checkIfStudentAlreadyRegistered != null)
                throw new InvalidOperationException($"Student with regNo {checkIfStudentExists.RegNo} has already registered to vote");

            string votingCode = VotingCodeExtention.StudentVotingCode();

            RegisteredVoter registerVoter = new()
            {
                StudentId = checkIfStudentExists.Id,
                VotingCode = votingCode,
                DepartmentId = checkIfStudentExists.DepartmentId,
            };

            await _registeredVoterRepo.AddAsync(registerVoter);

            return "Voter registration was successful";
        }
    }
}
