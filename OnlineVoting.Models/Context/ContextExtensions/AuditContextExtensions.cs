using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.ContextExtensions;

namespace OnlineVoting.Models.ContextExtensions
{
    public static class AuditContextExtensions
    {
        private static readonly HashSet<string> ExcludedAuditProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "RowVersion",
            "VotingCode",
            "CreatedAt",
            "UpdatedAt",
            "CreatedBy",
            "UpdatedBy",
            "NormalizedEmail",
            "NormalizedUserName"
        };

        public static List<PendingAuditEntry> PrepareAuditEntries(this ChangeTracker changeTracker)
        {
            List<EntityEntry> entries = GetAuditableEntries(changeTracker);

            List<PendingAuditEntry> auditEntries = new();

            foreach (EntityEntry entry in entries)
            {
                PropertyValues? databaseValues = null;

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    databaseValues = entry.GetDatabaseValues();

                PendingAuditEntry auditEntry = CreatePendingAuditEntry(entry, databaseValues);

                if (entry.State == EntityState.Modified
                    && auditEntry.OldValues.Count == 0
                    && auditEntry.NewValues.Count == 0)
                    continue;

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        public static bool HasAuditableChanges(this ChangeTracker changeTracker)
        {
            return changeTracker.Entries()
                .Any(entry => entry.Entity is IAuditable
                    && entry.Entity is not AuditTrail
                    && (entry.State == EntityState.Added
                        || entry.State == EntityState.Modified
                        || entry.State == EntityState.Deleted));
        }

        public static async Task<List<PendingAuditEntry>> PrepareAuditEntries(this ChangeTracker changeTracker, CancellationToken cancellationToken)
        {
            List<EntityEntry> entries = GetAuditableEntries(changeTracker);

            List<PendingAuditEntry> auditEntries = new();

            foreach (EntityEntry entry in entries)
            {
                PropertyValues? databaseValues = null;

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                PendingAuditEntry auditEntry = CreatePendingAuditEntry(entry, databaseValues);

                if (entry.State == EntityState.Modified
                    && auditEntry.OldValues.Count == 0
                    && auditEntry.NewValues.Count == 0)
                    continue;

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        public static string GetEventName(this EntityState state)
        {
            return state switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => state.ToString()
            };
        }

        public static string? GetEntityId(this EntityEntry entry, bool useOriginalValues = false)
        {
            IKey? primaryKey = entry.Metadata.FindPrimaryKey();

            if (primaryKey == null)
                return null;

            IEnumerable<string> values = primaryKey.Properties.Select(property =>
            {
                PropertyEntry propertyEntry = entry.Property(property.Name);

                object? value = useOriginalValues
                    ? propertyEntry.OriginalValue
                    : propertyEntry.CurrentValue;

                return value?.ToString() ?? string.Empty;
            });

            return string.Join(",", values);
        }

        private static List<EntityEntry> GetAuditableEntries(ChangeTracker changeTracker)
        {
            return changeTracker.Entries()
                .Where(entry => entry.Entity is IAuditable
                    && entry.Entity is not AuditTrail
                    && (entry.State == EntityState.Added
                        || entry.State == EntityState.Modified
                        || entry.State == EntityState.Deleted))
                .ToList();
        }

        private static PendingAuditEntry CreatePendingAuditEntry(EntityEntry entry, PropertyValues? databaseValues = null)
        {
            PendingAuditEntry pendingAuditEntry = new PendingAuditEntry
            {
                Entry = entry,
                State = entry.State,
                EntityType = entry.Metadata.ClrType.Name
            };

            if (entry.State != EntityState.Added)
                pendingAuditEntry.EntityId = entry.GetEntityId(entry.State == EntityState.Deleted);

            foreach (PropertyEntry property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey() || IsExcludedAuditProperty(propertyName))
                    continue;

                object? originalValue = databaseValues?[propertyName] ?? property.OriginalValue;
                object? currentValue = property.CurrentValue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        pendingAuditEntry.NewValues[propertyName] = currentValue;
                        break;

                    case EntityState.Modified:
                        if (!Equals(originalValue, currentValue))
                        {
                            pendingAuditEntry.OldValues[propertyName] = originalValue;
                            pendingAuditEntry.NewValues[propertyName] = currentValue;
                        }

                        break;

                    case EntityState.Deleted:
                        pendingAuditEntry.OldValues[propertyName] = originalValue;
                        break;
                }
            }

            return pendingAuditEntry;
        }

        private static bool IsExcludedAuditProperty(string propertyName)
        {
            if (ExcludedAuditProperties.Contains(propertyName))
                return true;

            if (propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase))
                return true;

            if (propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}