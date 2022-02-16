using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.GlobalMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Interfaces
{
    public interface IUserService
    {
        Task<Response> CreateUser(UserCreateRequestDto model);
    }
}
