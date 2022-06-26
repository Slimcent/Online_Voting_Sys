using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Interfaces
{
    public interface IVoterService
    {
        Task<string> CreateVoter(VoterCreateDto request);
        Task<string> ToggleVoter(Guid id);
    }
}
