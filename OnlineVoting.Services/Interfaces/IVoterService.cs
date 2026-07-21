using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Interfaces
{
    public interface IVoterService
    {
        Task<string> CreateVoter(CreateVoterRequest request);
        Task<string> ToggleVoter(Guid id);
    }
}