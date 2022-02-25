using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
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
        Task<UserClaimsResponseDto> CreateUserClaims(UserClaimsRequestDto request);
        Task<string> DeleteClaims(UserClaimsRequestDto request);
        Task<EditUserClaimsDto> EditUserClaims(EditUserClaimsDto userClaimsDto);
        Task<IEnumerable<UserClaimsResponseDto>> GetUserClaims(string email);
    }
}
