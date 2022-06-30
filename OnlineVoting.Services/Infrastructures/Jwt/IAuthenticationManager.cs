using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public interface IAuthenticationManager
    {
        Task<string> CreateToken();
    }
}
