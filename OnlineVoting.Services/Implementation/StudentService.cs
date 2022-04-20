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

            var userExists = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
            if (userExists != null)
                throw new UserExistException(model.Email);

            var regNumberExists = await _studentRepo.GetSingleByAsync(r => r.RegNo == model.RegNo);
            if (regNumberExists != null)
                throw new RegNoExistException(model.RegNo);

            var user = _mapper.Map<User>(model);
            user.EmailConfirmed = true;

            var password = "123456";
            var res = await _userManager.CreateAsync(user, password);

            if (!res.Succeeded)
                return new Response(false, "User creation failed");

            if (!_roleManager.RoleExistsAsync("Student").Result)
            {
                var role = new Role { Name = "Student" };
                var roleResult = _roleManager.CreateAsync(role).Result;

                if (!roleResult.Succeeded)
                    return new Response(false, "Error while creating role");                
            }
            await _userManager.AddToRoleAsync(user, "Student");

            var service = await _serviceFactory.GetService<IUserService>().CreateUserClaims(model.Email, ClaimTypes.Role, model.ClaimValue);

            var student = new Student { UserId = user.Id, RegNo = model.RegNo, FirstName = model.FirstName, LastName = model.LastName };
            await _studentRepo.AddAsync(student);
            var add = await _unitOfWork.SaveChangesAsync();

            if (add > 0) return new Response(true, "student created");
            return new Response(true, $"Student with email {model.Email} created successfully");
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
