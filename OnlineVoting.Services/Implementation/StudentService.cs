using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Services.Utilities;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;

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

            var contestantExists = await _contestantRepo.GetSingleByAsync(r => r.Student.RegNumber == regNo, include: r => r.Include(s => s.Student));
            if (contestantExists != null)
                throw new ConflictException(regNo);

            Student student = await _studentRepo.GetSingleByAsync(s => s.RegNumber == regNo, include: s => s.Include(u => u.User));
            if (student == null)
                throw new NotFoundException(regNo);

            var contestant = new Contestant
            {
                StudentId = student.Id,
                //UserId = student.UserId
            };
            await _contestantRepo.AddAsync(contestant);

            return new Response(true, $"Contestant with RegNumber {regNo} created successfully");
        }

        public async Task<Response> CreateStudent(CreateStudentRequest request)
        {
            if (request == null)
                throw new InvalidOperationException("Invalid data sent");

            //var regNumberExists = await _studentRepo.GetSingleByAsync(r => r.RegNumber == request.RegNumber);
            //if (regNumberExists != null)
            //    throw new ConflictException(request.RegNumber);

            //CreateUserRequest user = new()
            //{
            //    Email = request.Email,
            //    FirstName = request.FirstName,
            //    Role = request.Role,
            //};

            CreateUserRequest user = _mapper.Map<CreateUserRequest>(request);

            var userId = await _serviceFactory.GetService<IUserService>().CreateUser(user);

            var student = _mapper.Map<Student>(request);
            student.UserId = userId;

            await _studentRepo.AddAsync(student);

            await _unitOfWork.SaveChangesAsync();

            return new Response(true, $"Student with email {request.Email} created successfully");
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

        public async Task<string> UploadStudents(UploadStudentRequest request)
        {
            //string[] requiredHeaders = new[] {"RegNumber", "FirstName", "LastName", "Email", "PhoneNumber", "Sex"};
            //string[] nullableFields = new[] {"SN", "PhoneNumber", "Sex"};

            List<Dictionary<string, string>> studentData = _fileDataExtractor.ExtractFromExcel(request.File, null, ignoreFields: request.IgnoreFields);
            studentData.ValidateFields(request.RequiredFields);

            IEnumerable<Student> studentsToUpload = DictionaryToObjectConverter.DictionaryToObjects<Student>(studentData);

            foreach (Student student in studentsToUpload)
            {
                User exisitingUser = await _userManager.FindByEmailAsync(student.RegNumber);

                if (exisitingUser != null)
                {
                    continue;
                }

                //CreateUserRequest user = new()
                //{
                //    //Email = student.Email,
                //    //FirstName = student.FirstName,
                //    Role = "Student"
                //};

                CreateUserRequest user = _mapper.Map<CreateUserRequest>(student);

                string userId = await _serviceFactory.GetService<IUserService>().CreateUser(user);
                student.UserId = userId;
            }
            await _studentRepo.AddRangeAsync(studentsToUpload);

            return "Students uploaded successfully";
        }

        public async Task<Response> Vote(VoteRequest request)
        {
            if (request == null)
                throw new InvalidOperationException("Invalid data sent");

            var voterExists = await _studentRepo.GetSingleByAsync(s => s.RegNumber == request.VoterRegNo);
            if (voterExists == null)
                throw new NotFoundException(request.VoterRegNo);

            var contestantExists = _studentRepo.GetSingleByAsync(s => s.RegNumber == request.ContestantRegNo);
            if (voterExists == null)
                throw new NotFoundException(request.ContestantRegNo);

            throw new NotImplementedException();
        }
    }
}