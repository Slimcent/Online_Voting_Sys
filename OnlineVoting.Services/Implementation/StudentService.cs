using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Services.Utilities;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Staff> _staffRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Contestant> _contestantRepo;
        private readonly IRepository<RegisteredVoter> _registeredVoterRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public StudentService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _contestantRepo = _unitOfWork.GetRepository<Contestant>();
            _staffRepo = _unitOfWork.GetRepository<Staff>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Result<Response>> CreateContestant(string regNo, string position)
        {
            if (string.IsNullOrWhiteSpace(regNo))
                return Result<Response>.ValidationError("Registration number cannot be empty");

            if (string.IsNullOrWhiteSpace(position))
                return Result<Response>.ValidationError("Position cannot be empty");

            Contestant contestantExists = await _contestantRepo.GetSingleByAsync(r => r.Student.RegNumber == regNo, include: r => r.Include(s => s.Student));
            if (contestantExists != null)
                return Result<Response>.Conflict($"Contestant with registration number {regNo} already exists");

            Student student = await _studentRepo.GetSingleByAsync(s => s.RegNumber == regNo, include: s => s.Include(u => u.User));
            if (student == null)
                return Result<Response>.NotFound($"Student with registration number {regNo} was not found");

            Contestant contestant = new()
            {
                StudentId = student.Id,
                //UserId = student.UserId
            };

            await _contestantRepo.AddAsync(contestant);

            Response response = new Response(true, $"Contestant with RegNumber {regNo} created successfully");

            return Result<Response>.Created(response);
        }

        public async Task<Result<Response>> CreateStudent(CreateStudentRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            if (request == null)
            {
                _loggerMessage.LogWarn("Student creation rejected because the request was null.");

                return Result<Response>.ValidationError("Invalid data sent");
            }

            _loggerMessage.LogInfo("Starting student creation.");

            Gender gender = await _unitOfWork.GetRepository<Gender>().GetSingleByAsync(x => x.Id == request.GenderId);
            if (gender == null)
            {
                _loggerMessage.LogWarn($"Student creation failed because the specified gender with ID {request.GenderId} was not found.");

                return Result<Response>.ValidationError("Invalid gender specified");
            }

            Department department = await _unitOfWork.GetRepository<Department>().GetSingleByAsync(x => x.Id == request.DepartmentId);
            if (department == null)
            {
                _loggerMessage.LogWarn($"Student creation failed because the specified department with ID {request.DepartmentId} was not found.");
                return Result<Response>.ValidationError("Invalid department specified");
            }

            UserType userType = await _unitOfWork.GetRepository<UserType>().GetSingleByAsync(x => x.Id == request.UserTypeId);
            if (userType == null)
            {
                _loggerMessage.LogWarn($"Student creation failed because the specified user type with ID {request.UserTypeId} was not found.");
                return Result<Response>.ValidationError("Invalid user type specified");
            }

            Student student = _mapper.Map<Student>(request);

            Result<string> userResult = await _serviceFactory.GetService<IUserService>().CreateUser(request, user =>
            {
                student.User = user;
                user.Student = student;
            });

            if (!userResult.IsSuccess)
            {
                _loggerMessage.LogWarn("Student creation failed during user creation.");

                return Result<Response>.FromFailure(userResult);
            }

            _loggerMessage.LogInfo($"Student created successfully for user {userResult.Value}.");

            Response response = new Response(true, $"Student with email {request.Email} created successfully");

            await _unitOfWork.CommitTransactionAsync();

            return Result<Response>.Created(response);
        }

        public async Task<FileStreamResponse> DownloadStudentsList()
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

        public async Task<Result<string>> UploadStudents(UploadStudentRequest request)
        {
            if (request == null)
            {
                _loggerMessage.LogWarn("Student bulk upload rejected because the request was null.");

                return Result<string>.ValidationError("Invalid data sent.");
            }

            _loggerMessage.LogInfo("Starting student bulk upload.");

            ExtractDataFromExcelRequest excelRequest = _mapper.Map<ExtractDataFromExcelRequest>(request);

            (List<Dictionary<string, string>> studentData, string[] errMsgs) = DataExtension.ReadFromExcel(excelRequest, _loggerMessage);
            if (errMsgs.Any())
            {
                string errMSg = string.Join('\n', errMsgs);
                throw new InvalidOperationException(errMSg);
            }

            if (!studentData.Any())
            {
                _loggerMessage.LogWarn("Student bulk upload rejected because no students were found.");

                return Result<string>.ValidationError("No students were found in the uploaded file.");
            }

            IEnumerable<Department> departments = await _unitOfWork.GetRepository<Department>().GetAllAsync();
            Dictionary<string, long> departmentDictionary = departments.ToDictionary(x => x.Name.NormalizeLookupValue(), x => x.Id);

            IEnumerable<Gender> genders = await _unitOfWork.GetRepository<Gender>().GetAllAsync();
            Dictionary<string, int> genderDictionary = genders.ToDictionary(x => x.Name.NormalizeGender(), x => x.Id);

            studentData.SetDepartmentIds(departmentDictionary);
            studentData.SetGenderIds(genderDictionary);

            List<CreateStudentRequest> studentsToUpload = DataExtension.DictionaryToObjects<CreateStudentRequest>(studentData).ToList();
            studentsToUpload.ValidateDepartmentIds(departments, _loggerMessage);
            studentsToUpload.ValidateGenderIds(genders, _loggerMessage);

            if (!studentsToUpload.Any())
            {
                _loggerMessage.LogWarn("Student bulk upload rejected because no students were found.");

                return Result<string>.ValidationError("No students were found in the uploaded file.");
            }

            string? duplicateRegNumber = studentsToUpload
                .GroupBy(student => student.RegNumber.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (duplicateRegNumber != null)
            {
                _loggerMessage.LogWarn($"Student bulk upload rejected because registration number {duplicateRegNumber} occurs more than once.");

                return Result<string>.Conflict($"Registration number {duplicateRegNumber} occurs more than once in the upload.");
            }

            List<string> registrationNumbers = studentsToUpload.Select(student => student.RegNumber.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            string? existingRegNumber = await _studentRepo.GetQueryable().AsNoTracking()
                .Where(student => registrationNumbers.Contains(student.RegNumber))
                .Select(student => student.RegNumber)
                .FirstOrDefaultAsync();

            if (existingRegNumber != null)
            {
                _loggerMessage.LogWarn($"Student bulk upload rejected because registration number {existingRegNumber} already exists.");

                return Result<string>.Conflict($"A student with registration number {existingRegNumber} already exists.");
            }

            UserType? userType = await _unitOfWork.GetRepository<UserType>()
                .GetSingleByAsync(userType => userType.Id == ApplicationConstants.UserTypes.StudentUserTypeId);

            if (userType == null)
            {
                _loggerMessage.LogWarn("Student bulk upload failed because the Student user type was not found.");

                return Result<string>.ValidationError("Student user type was not found.");
            }

            foreach (CreateStudentRequest student in studentsToUpload)
            {
                student.RoleId = ApplicationConstants.Roles.StudentRoleId;
                student.UserTypeId = ApplicationConstants.UserTypes.StudentUserTypeId;
            }

            IUserService userService = _serviceFactory.GetService<IUserService>();

            Result<string> userResult = await userService.CreateUsers(studentsToUpload, (studentRequest, user) =>
            {
                Student student = _mapper.Map<Student>(studentRequest);

                student.User = user;
                user.Student = student;
            });

            if (!userResult.IsSuccess)
            {
                _loggerMessage.LogWarn($"Student bulk upload failed. {userResult.Error}");

                return Result<string>.FromFailure(userResult);
            }

            _loggerMessage.LogInfo($"Student bulk upload completed successfully. Created {studentsToUpload.Count} students.");

            return Result<string>.Created($"{studentsToUpload.Count} students uploaded successfully.");
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

        public async Task<int> UpdateInactiveStudents()
        {
            _loggerMessage.LogInfo("Starting inactive student update.");

            List<Student> students = await _studentRepo.GetQueryable(x => !x.Active && x.User != null).ToListAsync();

            if (students.Count == 0)
            {
                _loggerMessage.LogInfo("No inactive student records were found.");

                return 0;
            }

            students.ForEach(x => x.Active = true);

            await _unitOfWork.SaveChangesAsync();

            _loggerMessage.LogInfo($"{students.Count} inactive student record(s) updated successfully.");

            return students.Count;
        }
    }
}