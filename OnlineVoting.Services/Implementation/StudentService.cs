using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Interfaces;
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
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Response> CreateStudent(StudentCreateRequestDto model)
        {
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

            var student = new Student { UserId = user.Id, RegNo = model.RegNo };
            await _studentRepo.AddAsync(student);
            var add = await _unitOfWork.SaveChangesAsync();

            if (add > 0) return new Response(true, "student created");
            return new Response(true, $"Student with email {model.Email} created successfully");
        }
    }
}
