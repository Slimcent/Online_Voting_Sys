using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<string> CreateFaculty(CreateFacultyDto request);
    }
}
