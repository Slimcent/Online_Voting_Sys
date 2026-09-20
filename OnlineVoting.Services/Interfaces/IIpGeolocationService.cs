using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IIpGeolocationService
    {
        Task<IpGeolocationResponse?> GetLocation(string ipAddress, CancellationToken cancellationToken = default(CancellationToken));
    }
}
