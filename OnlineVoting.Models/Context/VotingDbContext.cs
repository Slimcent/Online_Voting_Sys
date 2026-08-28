using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineVoting.Models.Context.ContextExtensions;
using OnlineVoting.Models.ContextExtensions;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Context
{
    public class VotingDbContext : IdentityDbContext<User, Role, string, ApplicationUserClaim, ApplicationUserRole,
        IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>
    {

        private readonly ICurrentUserContext _currentUserContext;
        private readonly IAuditMetadataProvider _auditMetadataProvider;
        
        public VotingDbContext(DbContextOptions<VotingDbContext> options, ICurrentUserContext currentUserContext, IAuditMetadataProvider auditMetadataProvider) : base(options)
        {
            _currentUserContext = currentUserContext;
            _auditMetadataProvider = auditMetadataProvider;
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();

            if (!ChangeTracker.HasAuditableChanges())
                return base.SaveChanges(true);

            bool ownsTransaction = Database.CurrentTransaction == null;

            using IDbContextTransaction? transaction = ownsTransaction ? Database.BeginTransaction() : null;

            try
            {
                List<PendingAuditEntry> auditEntries = ChangeTracker.PrepareAuditEntries();

                int result = base.SaveChanges(true);

                auditEntries.ResolveGeneratedEntityIds();

                int outcomeId = AuditOutcomes.GetSuccessAuditOutcomeId();

                List<AuditTrail> auditTrails = auditEntries.CreateAuditTrails(_auditMetadataProvider, outcomeId);

                if (auditTrails.Count > 0)
                {
                    AuditTrails.AddRange(auditTrails);
                    base.SaveChanges(true);
                }

                if (ownsTransaction)
                    transaction!.Commit();

                return result;
            }
            catch
            {
                if (ownsTransaction && transaction != null)
                    transaction.Rollback();

                throw;
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (acceptAllChangesOnSuccess)
                return SaveChanges();

            OnBeforeSaving();
            return base.SaveChanges(false);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (acceptAllChangesOnSuccess)
                return SaveChangesAsync(cancellationToken);

            OnBeforeSaving();
            return base.SaveChangesAsync(false, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();

            if (!ChangeTracker.HasAuditableChanges())
                return await base.SaveChangesAsync(true, cancellationToken);

            bool ownsTransaction = Database.CurrentTransaction == null;

            await using IDbContextTransaction? transaction = ownsTransaction
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;

            try
            {
                List<PendingAuditEntry> auditEntries = await ChangeTracker.PrepareAuditEntries(cancellationToken);

                int result = await base.SaveChangesAsync(true, cancellationToken);

                auditEntries.ResolveGeneratedEntityIds();

                int outcomeId = await AuditOutcomes.GetSuccessAuditOutcomeId(cancellationToken);

                List<AuditTrail> auditTrails = auditEntries.CreateAuditTrails(_auditMetadataProvider, outcomeId);

                if (auditTrails.Count > 0)
                {
                    AuditTrails.AddRange(auditTrails);
                    await base.SaveChangesAsync(true, cancellationToken);
                }

                if (ownsTransaction)
                    await transaction!.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                if (ownsTransaction && transaction != null)
                    await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        }

        private void OnBeforeSaving()
        {
            IEnumerable<EntityEntry> entries = ChangeTracker.Entries();
            string? username = _currentUserContext.Username;

            foreach (EntityEntry entry in entries)
            {
                if (entry.Entity is AuditTrail auditTrail)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        throw new InvalidOperationException("Audit trail records cannot be modified or deleted.");
                    }

                    if (entry.State == EntityState.Added)
                    {
                        auditTrail.CreatedAt = DateTime.UtcNow;
                    }
                }

                if (entry.Entity is AuditLocation)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        throw new InvalidOperationException("Audit location records cannot be modified or deleted.");
                    }
                }

                if (entry.Entity is ITracker trackable)
                {
                    DateTime now = DateTime.UtcNow;

                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedAt = now;
                            trackable.UpdatedBy = username;
                            break;

                        case EntityState.Added:
                            trackable.CreatedAt = now;
                            trackable.UpdatedAt = now;
                            trackable.CreatedBy = username;
                            trackable.UpdatedBy = username;
                            break;
                    }
                }
            }
        }
                
        public DbSet<Student> Students { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Contestant> Contestants { get; set; }
        public DbSet<Position> Positions { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<Staff> StaffProfile { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<AuditOutcome> AuditOutcomes { get; set; }
        public DbSet<AuditLocation> AuditLocations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();


                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                b.HasMany(e => e.RefreshTokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(refreshToken => refreshToken.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasIndex(e => e.TokenHash)
                    .IsUnique();

                b.HasIndex(e => e.FamilyId);

                b.Property(e => e.TokenHash)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(e => e.FamilyId)
                    .IsRequired()
                    .HasMaxLength(32);

                b.Property(e => e.ReplacedByTokenHash)
                    .HasMaxLength(64);

                b.Property(e => e.RevokedReason)
                    .HasMaxLength(200);

                b.Property(e => e.CreatedByIp)
                    .HasMaxLength(45);

                b.Property(e => e.RevokedByIp)
                    .HasMaxLength(45);

                b.Property(e => e.UserAgent)
                    .HasMaxLength(512);

                b.Property(e => e.UserId)
                    .IsRequired();

                b.Property(e => e.RowVersion)
                    .IsRowVersion();
            });

            modelBuilder.Entity<AuditOutcome>(b =>
            {
                b.HasIndex(e => e.Name)
                    .IsUnique();

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Description)
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<AuditTrail>(b =>
            {
                b.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(36)
                    .ValueGeneratedNever();

                b.Property(e => e.ActorUserId)
                    .HasMaxLength(450);

                b.Property(e => e.ActorUsername)
                    .HasMaxLength(256);

                b.Property(e => e.EndpointName)
                    .HasMaxLength(200);

                b.Property(e => e.EventName)
                    .HasMaxLength(200);

                b.Property(e => e.HttpMethod)
                    .HasMaxLength(10);

                b.Property(e => e.EntityType)
                    .HasMaxLength(100);

                b.Property(e => e.EntityId)
                    .HasMaxLength(450);

                b.Property(e => e.Description)
                    .HasMaxLength(1000);

                b.Property(e => e.IpAddress)
                    .HasMaxLength(45);

                b.Property(e => e.UserAgent)
                    .HasMaxLength(512);

                b.Property(e => e.CorrelationId)
                    .HasMaxLength(100);

                b.Property(e => e.CreatedAt)
                    .IsRequired();

                b.HasOne(e => e.Outcome)
                    .WithMany(e => e.AuditTrails)
                    .HasForeignKey(e => e.OutcomeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasIndex(e => e.CreatedAt);

                b.HasIndex(e => e.ActorUserId);

                b.HasIndex(e => e.EndpointName);

                b.HasIndex(e => e.OutcomeId);

                b.HasIndex(e => new
                {
                    e.EntityType,
                    e.EntityId
                });

                b.HasIndex(e => e.CorrelationId);
            });

            modelBuilder.Entity<AuditLocation>(b =>
            {
                b.HasKey(e => e.Id);

                b.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(36)
                    .ValueGeneratedNever();

                b.Property(e => e.AuditTrailId)
                    .IsRequired()
                    .HasMaxLength(36);

                b.Property(e => e.IpCountry)
                    .HasMaxLength(100);

                b.Property(e => e.IpRegion)
                    .HasMaxLength(150);

                b.Property(e => e.IpCity)
                    .HasMaxLength(150);

                b.HasOne(e => e.AuditTrail)
                    .WithOne(e => e.Location)
                    .HasForeignKey<AuditLocation>(e => e.AuditTrailId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasIndex(e => e.AuditTrailId)
                    .IsUnique();
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });            
        }
    }
}
