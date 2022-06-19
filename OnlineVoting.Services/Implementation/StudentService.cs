using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using VotingSystem.Data.Interfaces;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Utilities;

namespace OnlineVoting.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Contestant> _contestantRepo;
        private readonly IFileDataExtractorService _fileDataExtractor;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _contestantRepo = _unitOfWork.GetRepository<Contestant>();
            _fileDataExtractor = _serviceFactory.GetService<IFileDataExtractorService>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Response> CreateContestant(string regNo, string position)
        {
            if (regNo == null)
                throw new InvalidOperationException("Invalid data sent");

            var contestantExists = await _contestantRepo.GetSingleByAsync(r => r.Student.RegNo == regNo, include: r => r.Include(s => s.Student));
            if (contestantExists != null)
                throw new RegNoExistException(regNo);

            var student = await _studentRepo.GetSingleByAsync(s => s.RegNo == regNo, include: s => s.Include(u => u.User));
            if (student == null)
                throw new RegNoNotFoundException(regNo);

            var contestant = new Contestant
            {
                StudentId = student.Id,
                //UserId = student.UserId
            };
            await _contestantRepo.AddAsync(contestant);

            return new Response(true, $"Contestant with RegNo {regNo} created successfully");
        }

        public async Task<Response> CreateStudent(StudentCreateRequestDto model)
        {
            if (model == null)
                throw new InvalidOperationException("Invalid data sent");
                        
            var regNumberExists = await _studentRepo.GetSingleByAsync(r => r.RegNo == model.RegNo);
            if (regNumberExists != null)
                throw new RegNoExistException(model.RegNo);

            UserCreateRequestDto user = new()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                Role = model.Role,
            };
            var userId = await _serviceFactory.GetService<IUserService>().CreateUser(user);

            var student = _mapper.Map<Student>(model);
            student.UserId = userId;
                        
            await _studentRepo.AddAsync(student);

            await _unitOfWork.SaveChangesAsync();

            return new Response(true, $"Student with email {model.Email} created successfully");
        }

        public async Task<FileStreamDto> DownloadStudentsList()
        {
            return new List<StudentListDownload>()
            {
                new()
                {
                    SN = 1,
                    RegNo = "Esut/2012/43569",
                    LastName = "Ach",
                    FirstName = "Obi",
                    Email = "achobi@example.com",
                    PhoneNumber = "09078786543",
                    Gender = "Male"
                }

            }.ConvertToExcel(new ExcelDownloadConfig { Name = "StudentList" });
        }

        public async Task<string> UploadStudents(UploadStudentRequestDto model)
        {
            //string[] requiredHeaders = new[] {"RegNo", "FirstName", "LastName", "Email", "PhoneNumber", "Sex"};
            //string[] nullableFields = new[] {"SN", "PhoneNumber", "Sex"};
                        
            List<Dictionary<string, string>> studentData = _fileDataExtractor.ExtractFromExcel(model.File, null, ignoreFields: model.IgnoreFields);
            studentData.ValidateFields(model.RequiredFields);

            IEnumerable<Student> studentsToUpload = DictionaryToObjectConverter.DictionaryToObjects<Student>(studentData);

            foreach (Student student in studentsToUpload)
            {
                User exisitingUser = await _userManager.FindByEmailAsync(student.Email);

                if (exisitingUser != null)
                {
                    continue;
                }

                UserCreateRequestDto user = new()
                {
                    Email = student.Email,
                    FirstName = student.FirstName,
                    Role = "Student"
                };

                string userId = await _serviceFactory.GetService<IUserService>().CreateUser(user);
                student.UserId = userId;
            }
            await _studentRepo.AddRangeAsync(studentsToUpload);

            return "Students uploaded successfully";
        }

        public async Task<Response> Vote(VoteRequestDto request)
        {
            if (request == null)
                throw new InvalidOperationException("Invalid data sent");

            var voterExists = await _studentRepo.GetSingleByAsync(s => s.RegNo == request.VoterRegNo);
            if (voterExists == null)
                throw new RegNoNotFoundException(request.VoterRegNo);

            var contestantExists = _studentRepo.GetSingleByAsync(s => s.RegNo == request.ContestantRegNo);
            if (voterExists == null)
                throw new RegNoNotFoundException(request.ContestantRegNo);

            throw new NotImplementedException();
        }
    }
}
