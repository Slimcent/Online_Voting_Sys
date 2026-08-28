using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class AuditTrailTestData
    {
        public static AuditTrail CreateAuditTrail(int outcomeId, string actorUserId = "user-id",
            string actorUsername = "super.admin", string endpointName = "Create-Faculty",
            string eventName = "Created", string httpMethod = "POST", string entityType = "Faculty",
            string entityId = "1", string correlationId = "create-correlation-id",
            string ipAddress = "127.0.0.1")
        {
            return new AuditTrail
            {
                ActorUserId = actorUserId,
                ActorUsername = actorUsername,
                EndpointName = endpointName,
                EventName = eventName,
                HttpMethod = httpMethod,
                EntityType = entityType,
                EntityId = entityId,
                OutcomeId = outcomeId,
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                UserAgent = "Test User Agent"
            };
        }

        public static List<AuditTrail> CreateAuditTrails(int outcomeId)
        {
            AuditTrail createFacultyAuditTrail = CreateAuditTrail(outcomeId);

            createFacultyAuditTrail.Location = new AuditLocation
            {
                AuditTrailId = createFacultyAuditTrail.Id,
                IpCountry = "Germany",
                IpRegion = "North Rhine-Westphalia",
                IpCity = "Paderborn",
                IpLatitude = 51.7189,
                IpLongitude = 8.7575,
                DeviceLatitude = 51.7190,
                DeviceLongitude = 8.7576,
                DeviceAccuracyMeters = 10.5,
                DeviceLocationCapturedAt = DateTime.UtcNow,
                AuditTrail = createFacultyAuditTrail
            };

            return new List<AuditTrail>
            {
                createFacultyAuditTrail,

                CreateAuditTrail(
                    outcomeId,
                    endpointName: "Update-Faculty",
                    eventName: "Updated",
                    httpMethod: "PUT",
                    correlationId: "update-correlation-id"),

                CreateAuditTrail(
                    outcomeId,
                    actorUserId: "audit-admin-id",
                    actorUsername: "audit.admin",
                    endpointName: "Delete-Faculty",
                    eventName: "Deleted",
                    httpMethod: "DELETE",
                    entityId: "2",
                    correlationId: "delete-correlation-id",
                    ipAddress: "10.0.0.2"),

                CreateAuditTrail(
                    outcomeId,
                    endpointName: "Create-Department",
                    entityType: "Department",
                    entityId: "3",
                    correlationId: "department-correlation-id")
            };
        }

        public static async Task<List<AuditTrail>> SeedAuditTrails(VotingDbContext context)
        {
            int outcomeId = await context.AuditOutcomes
                .Where(auditOutcome => auditOutcome.Name == "Success")
                .Select(auditOutcome => auditOutcome.Id)
                .SingleAsync();

            List<AuditTrail> auditTrails = CreateAuditTrails(outcomeId);

            await context.AuditTrails.AddRangeAsync(auditTrails);
            await context.SaveChangesAsync();

            return auditTrails;
        }
    }
}