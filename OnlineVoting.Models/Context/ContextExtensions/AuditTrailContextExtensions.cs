using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using System.Text.Json;

namespace OnlineVoting.Models.ContextExtensions
{
    public static class AuditTrailContextExtensions
    {
        public static void ResolveGeneratedEntityIds(this List<PendingAuditEntry> auditEntries)
        {
            foreach (PendingAuditEntry auditEntry in auditEntries.Where(auditEntry => auditEntry.State == EntityState.Added))
                auditEntry.EntityId = auditEntry.Entry.GetEntityId();
        }

        public static List<AuditTrail> CreateAuditTrails(this List<PendingAuditEntry> auditEntries, IAuditMetadataProvider auditMetadataProvider, int outcomeId)
        {
            string? actorUserId = auditMetadataProvider.GetActorUserId();
            string? actorUsername = auditMetadataProvider.GetActorUsername();
            string? endpointName = auditMetadataProvider.GetEndpointName();
            string? httpMethod = auditMetadataProvider.GetHttpMethod();
            string? ipAddress = auditMetadataProvider.GetIpAddress();
            string? userAgent = auditMetadataProvider.GetUserAgent();
            string? correlationId = auditMetadataProvider.GetCorrelationId();

            string? ipCountry = auditMetadataProvider.GetIpCountry();
            string? ipRegion = auditMetadataProvider.GetIpRegion();
            string? ipCity = auditMetadataProvider.GetIpCity();
            double? ipLatitude = auditMetadataProvider.GetIpLatitude();
            double? ipLongitude = auditMetadataProvider.GetIpLongitude();

            double? deviceLatitude = auditMetadataProvider.GetDeviceLatitude();
            double? deviceLongitude = auditMetadataProvider.GetDeviceLongitude();
            double? deviceAccuracyMeters = auditMetadataProvider.GetDeviceAccuracyMeters();
            DateTime? deviceLocationCapturedAt = auditMetadataProvider.GetDeviceLocationCapturedAt();

            bool hasIpLocation = !string.IsNullOrWhiteSpace(ipCountry)
                || !string.IsNullOrWhiteSpace(ipRegion)
                || !string.IsNullOrWhiteSpace(ipCity)
                || (ipLatitude.HasValue && ipLongitude.HasValue);

            bool hasDeviceLocation = deviceLatitude.HasValue && deviceLongitude.HasValue;

            List<AuditTrail> auditTrails = new();

            foreach (PendingAuditEntry auditEntry in auditEntries)
            {
                AuditTrail auditTrail = new()
                {
                    ActorUserId = actorUserId,
                    ActorUsername = actorUsername,
                    EndpointName = endpointName,
                    EventName = auditEntry.State.GetEventName(),
                    HttpMethod = httpMethod,
                    EntityType = auditEntry.EntityType,
                    EntityId = auditEntry.EntityId,
                    OutcomeId = outcomeId,
                    Description = BuildDescription(auditEntry, actorUsername),
                    OldValues = SerializeValues(auditEntry.OldValues),
                    NewValues = SerializeValues(auditEntry.NewValues),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CorrelationId = correlationId,
                    CreatedAt = DateTime.UtcNow
                };

                if (hasIpLocation || hasDeviceLocation)
                {
                    auditTrail.Location = new AuditLocation
                    {
                        AuditTrailId = auditTrail.Id,
                        IpCountry = ipCountry,
                        IpRegion = ipRegion,
                        IpCity = ipCity,
                        IpLatitude = ipLatitude,
                        IpLongitude = ipLongitude,
                        DeviceLatitude = deviceLatitude,
                        DeviceLongitude = deviceLongitude,
                        DeviceAccuracyMeters = deviceAccuracyMeters,
                        DeviceLocationCapturedAt = deviceLocationCapturedAt,
                        AuditTrail = auditTrail
                    };
                }

                auditTrails.Add(auditTrail);
            }

            return auditTrails;
        }

        private static string? SerializeValues(Dictionary<string, object?> values)
        {
            if (values.Count == 0)
                return null;

            return JsonSerializer.Serialize(values);
        }

        private static string BuildDescription(PendingAuditEntry auditEntry, string? actorUsername)
        {
            string entity = string.IsNullOrWhiteSpace(auditEntry.EntityId)
                ? auditEntry.EntityType
                : $"{auditEntry.EntityType} {auditEntry.EntityId}";

            string actor = string.IsNullOrWhiteSpace(actorUsername)
                ? "the system"
                : actorUsername;

            string eventName = auditEntry.State.GetEventName().ToLowerInvariant();

            return $"{entity} was {eventName} by {actor}.";
        }
    }
}