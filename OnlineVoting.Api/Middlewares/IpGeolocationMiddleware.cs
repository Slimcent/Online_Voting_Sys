using OnlineVoting.Models.Configurations;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace OnlineVoting.Api.Middlewares
{
    public class IpGeolocationMiddleware
    {
        private readonly RequestDelegate _next;

        public IpGeolocationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIpGeolocationService ipGeolocationService)
        {
            if (!ShouldResolveLocation(context.Request))
            {
                await _next(context);

                return;
            }

            IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

            if (remoteIpAddress == null)
            {
                await _next(context);

                return;
            }

            if (remoteIpAddress.IsIPv4MappedToIPv6)
                remoteIpAddress = remoteIpAddress.MapToIPv4();

            if (IsPrivateOrReserved(remoteIpAddress))
            {
                await _next(context);

                return;
            }

            IpGeolocationResponse? location = await ipGeolocationService.GetLocation(remoteIpAddress.ToString(), context.RequestAborted);

            if (location != null)
            {
                if (!string.IsNullOrWhiteSpace(location.Country))
                    context.Items[RequestContextKeys.IpCountry] = location.Country;

                if (!string.IsNullOrWhiteSpace(location.Region))
                    context.Items[RequestContextKeys.IpRegion] = location.Region;

                if (!string.IsNullOrWhiteSpace(location.City))
                    context.Items[RequestContextKeys.IpCity] = location.City;

                if (location.Latitude.HasValue)
                    context.Items[RequestContextKeys.IpLatitude] = location.Latitude.Value;

                if (location.Longitude.HasValue)
                    context.Items[RequestContextKeys.IpLongitude] = location.Longitude.Value;
            }

            await _next(context);
        }

        private static bool ShouldResolveLocation(HttpRequest request)
        {
            if (!request.Path.StartsWithSegments("/api"))
                return false;

            return HttpMethods.IsPost(request.Method)
                || HttpMethods.IsPut(request.Method)
                || HttpMethods.IsPatch(request.Method)
                || HttpMethods.IsDelete(request.Method);
        }

        private static bool IsPrivateOrReserved(IPAddress ipAddress)
        {
            if (IPAddress.IsLoopback(ipAddress))
                return true;

            if (ipAddress.Equals(IPAddress.Any) || ipAddress.Equals(IPAddress.IPv6Any))
                return true;

            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = ipAddress.GetAddressBytes();

                return bytes[0] == 10
                    || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    || (bytes[0] == 192 && bytes[1] == 168)
                    || (bytes[0] == 169 && bytes[1] == 254)
                    || (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
                    || bytes[0] == 0
                    || bytes[0] >= 224;
            }

            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                byte[] bytes = ipAddress.GetAddressBytes();

                return ipAddress.IsIPv6LinkLocal
                    || ipAddress.IsIPv6Multicast
                    || (bytes[0] & 0xFE) == 0xFC;
            }

            return true;
        }
    }
}