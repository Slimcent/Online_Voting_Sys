using Microsoft.Extensions.Caching.Memory;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class IpGeolocationService : IIpGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILoggerMessage _loggerMessage;
        private readonly IServiceFactory _serviceFactory;

        public IpGeolocationService(HttpClient httpClient, IMemoryCache memoryCache, IServiceFactory serviceFactory)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _serviceFactory = serviceFactory;
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<IpGeolocationResponse?> GetLocation(string ipAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            string cacheKey = $"ip-location:{ipAddress}";

            if (_memoryCache.TryGetValue(cacheKey, out IpGeolocationResponse? cachedLocation))
                return cachedLocation;

            try
            {
                string encodedIpAddress = Uri.EscapeDataString(ipAddress);

                IpGeolocationResponse? response = await _httpClient.GetFromJsonAsync<IpGeolocationResponse>(
                    $"{encodedIpAddress}?fields=success,message,country,region,city,latitude,longitude",
                    cancellationToken);

                if (response == null || !response.Success)
                {
                    _loggerMessage.LogWarn($"IP geolocation lookup was unsuccessful for IP {ipAddress}. "
                        + $"Message: {response?.Message ?? "No response"}");

                    return null;
                }

                _memoryCache.Set(cacheKey, response, TimeSpan.FromHours(6));

                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _loggerMessage.LogWarn($"IP geolocation lookup timed out for IP {ipAddress}.");

                return null;
            }
            catch (HttpRequestException exception)
            {
                _loggerMessage.LogWarn($"IP geolocation lookup failed for IP {ipAddress}. "
                    + $"Message: {exception.Message}");

                return null;
            }
            catch (JsonException exception)
            {
                _loggerMessage.LogWarn($"IP geolocation returned an invalid response for IP {ipAddress}. "
                    + $"Message: {exception.Message}");

                return null;
            }
        }
    }
}
