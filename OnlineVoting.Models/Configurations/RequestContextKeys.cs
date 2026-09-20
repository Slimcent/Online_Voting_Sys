using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineVoting.Models.Configurations
{
    public static class RequestContextKeys
    {
        public const string CorrelationId = "CorrelationId";
        public const string IpCountry = "IpCountry";
        public const string IpRegion = "IpRegion";
        public const string IpCity = "IpCity";
        public const string IpLatitude = "IpLatitude";
        public const string IpLongitude = "IpLongitude";
    }
}
