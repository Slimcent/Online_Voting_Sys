using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
        }

        public async Task<Response> CreateUser(UserCreateRequestDto model)
        {
            var user = new User
            {
                FullName = $"{model.FirstName} {model.LastName}",
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email
            };
            var password = "123456";

            var res = await _userManager.CreateAsync(user, password);

            if (!res.Succeeded)
            {
                return new Response(false, "User creation failed");
            }

            if (!_roleManager.RoleExistsAsync("Staff").Result)
            {
                var role = new Role
                {
                    Name = "Staff"
                };
                var roleResult = _roleManager.CreateAsync(role).Result;
                if (!roleResult.Succeeded)
                {
                    return new Response(false, "Error while creating role");
                }
            }
            await _userManager.AddToRoleAsync(user, "Staff");

            return new Response(true, $"User with email {model.Email} created successfully");

        }
    }
}
