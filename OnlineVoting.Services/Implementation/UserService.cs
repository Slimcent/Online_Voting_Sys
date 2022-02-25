using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
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
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Response> CreateUser(UserCreateRequestDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());

            if (existingUser != null)
                throw new UserExistException(model.Email);

            var user = _mapper.Map<User>(model);
            user.EmailConfirmed = true;

            var password = "123456";
            var res = await _userManager.CreateAsync(user, password);

            if (!res.Succeeded)
            {
                throw new InvalidOperationException($"User creation failed");
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
                    throw new InvalidOperationException($"Role creation failed");
                }
            }
            await _userManager.AddToRoleAsync(user, "Staff");

            return new Response(true, $"User with email {user.Email} created successfully");
        }
    }
}
