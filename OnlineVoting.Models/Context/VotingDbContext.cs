using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineVoting.Models.Context.ContextExtensions;
using OnlineVoting.Models.ContextExtensions;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
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
        public DbSet<Year> Years { get; set; }
        public DbSet<Contestant> Contestants { get; set; }
        public DbSet<Position> Positions { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<Staff> StaffProfile { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<RegisteredVoter> RegisteredVoters { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<AuditOutcome> AuditOutcomes { get; set; }
        public DbSet<AuditLocation> AuditLocations { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<ElectionType> ElectionTypes { get; set; }
        public DbSet<ElectionScope> ElectionScopes { get; set; }
        public DbSet<ElectionStatus> ElectionStatuses { get; set; }
        public DbSet<ElectionPosition> ElectionPositions { get; set; }
        public DbSet<PositionApplicationStatus> PositionApplicationStatuses { get; set; }
        public DbSet<PositionApplication> PositionApplications { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceStatus> InvoiceStatuses { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<PaymentGateway> PaymentGateways { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }


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

                b.HasIndex(e => e.NormalizedEmail)
                    .HasDatabaseName("EmailIndex")
                    .IsUnique()
                    .HasFilter("[NormalizedEmail] IS NOT NULL");
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

            modelBuilder.Entity<Address>(b =>
            {
                b.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                b.Property(e => e.StreetName)
                    .HasMaxLength(200);

                b.Property(e => e.City)
                    .HasMaxLength(100);

                b.Property(e => e.State)
                    .HasMaxLength(100);

                b.Property(e => e.Nationality)
                    .HasMaxLength(100);

                b.HasIndex(e => e.UserId)
                    .IsUnique();

                b.HasOne(e => e.User)
                    .WithOne(e => e.Address)
                    .HasForeignKey<Address>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity<Student>(b =>
            {
                b.Property(e => e.RegNumber)
                    .HasMaxLength(50);

                b.HasIndex(e => e.RegNumber)
                    .IsUnique()
                    .HasFilter("[RegNumber] IS NOT NULL");
            });

            modelBuilder.Entity<Faculty>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Department>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                b.HasIndex(e => new
                {
                    e.FacultyId,
                    e.Name
                })
                .IsUnique();
            });

            modelBuilder.Entity<Gender>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<UserType>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Year>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20);

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<ElectionType>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Name)
                    .IsUnique();

                b.HasOne(e => e.ElectionScope)
                    .WithMany(e => e.ElectionTypes)
                    .HasForeignKey(e => e.ElectionScopeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ElectionScope>(b =>
            {
                b.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Code)
                    .IsUnique();

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<ElectionStatus>(b =>
            {
                b.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Code)
                    .IsUnique();

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Position>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Positions_Scope",
                    "[FacultyId] IS NULL OR [DepartmentId] IS NULL"));

                b.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasFilter("[FacultyId] IS NULL AND [DepartmentId] IS NULL");

                b.HasIndex(e => new { e.FacultyId, e.Name })
                    .IsUnique()
                    .HasFilter("[FacultyId] IS NOT NULL AND [DepartmentId] IS NULL");

                b.HasIndex(e => new { e.DepartmentId, e.Name })
                    .IsUnique()
                    .HasFilter("[DepartmentId] IS NOT NULL AND [FacultyId] IS NULL");

                b.HasOne(e => e.Faculty)
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ElectionPosition>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ElectionId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.PositionId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ApplicationFee)
                    .HasPrecision(18, 2);

                b.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                b.HasIndex(e => new { e.ElectionId, e.PositionId })
                    .IsUnique();

                b.ToTable(t => t.HasCheckConstraint(
                    "CK_ElectionPositions_ApplicationFee",
                    "[ApplicationFee] >= 0"));

                b.HasOne(e => e.Election)
                    .WithMany(e => e.ElectionPositions)
                    .HasForeignKey(e => e.ElectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Position)
                    .WithMany(e => e.ElectionPositions)
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PositionApplicationStatus>(b =>
            {
                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasIndex(x => x.Code)
                    .IsUnique();

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<PositionApplication>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ElectionPositionId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.HasIndex(e => new { e.StudentId, e.ElectionPositionId })
                    .IsUnique();

                b.HasOne(e => e.Student)
                    .WithMany(e => e.PositionApplications)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.ElectionPosition)
                    .WithMany(e => e.Applications)
                    .HasForeignKey(e => e.ElectionPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.PositionApplicationStatus)
                    .WithMany(e => e.Applications)
                    .HasForeignKey(e => e.PositionApplicationStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Contestant>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.PositionApplicationId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.HasIndex(e => e.PositionApplicationId)
                    .IsUnique();

                b.HasOne(e => e.PositionApplication)
                    .WithOne(e => e.Contestant)
                    .HasForeignKey<Contestant>(e => e.PositionApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Election>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                b.HasIndex(e => new { e.ElectionTypeId, e.YearId })
                    .IsUnique()
                    .HasFilter("[FacultyId] IS NULL AND [DepartmentId] IS NULL");

                b.HasIndex(e => new { e.FacultyId, e.ElectionTypeId, e.YearId })
                    .IsUnique()
                    .HasFilter("[FacultyId] IS NOT NULL AND [DepartmentId] IS NULL");

                b.HasIndex(e => new { e.DepartmentId, e.ElectionTypeId, e.YearId })
                    .IsUnique()
                    .HasFilter("[DepartmentId] IS NOT NULL AND [FacultyId] IS NULL");

                b.HasOne(e => e.Year)
                    .WithMany(e => e.Elections)
                    .HasForeignKey(e => e.YearId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.ElectionType)
                    .WithMany(e => e.Elections)
                    .HasForeignKey(e => e.ElectionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.ElectionStatus)
                    .WithMany(e => e.Elections)
                    .HasForeignKey(e => e.ElectionStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Faculty)
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
                                
                b.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Elections_Scope",
                        "[FacultyId] IS NULL OR [DepartmentId] IS NULL");

                    t.HasCheckConstraint(
                        "CK_Elections_ApplicationPeriod",
                        "([ApplicationStartAt] IS NULL AND [ApplicationEndAt] IS NULL) OR " +
                        "([ApplicationStartAt] IS NOT NULL AND [ApplicationEndAt] IS NOT NULL AND " +
                        "[ApplicationEndAt] > [ApplicationStartAt])");

                    t.HasCheckConstraint(
                        "CK_Elections_VoterRegistrationPeriod",
                        "([VoterRegistrationStartAt] IS NULL AND [VoterRegistrationEndAt] IS NULL) OR " +
                        "([VoterRegistrationStartAt] IS NOT NULL AND [VoterRegistrationEndAt] IS NOT NULL AND " +
                        "[VoterRegistrationEndAt] > [VoterRegistrationStartAt])");

                    t.HasCheckConstraint(
                        "CK_Elections_VotingPeriod",
                        "([VotingStartAt] IS NULL AND [VotingEndAt] IS NULL) OR " +
                        "([VotingStartAt] IS NOT NULL AND [VotingEndAt] IS NOT NULL AND " +
                        "[VotingEndAt] > [VotingStartAt])");
                });
            });

            modelBuilder.Entity<Vote>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.RegisteredVoterId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ContestantId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ElectionPositionId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.HasIndex(e => new
                {
                    e.RegisteredVoterId,
                    e.ElectionPositionId
                })
                .IsUnique();

                b.HasOne(e => e.RegisteredVoter)
                    .WithMany(e => e.Votes)
                    .HasForeignKey(e => e.RegisteredVoterId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Contestant)
                    .WithMany()
                    .HasForeignKey(e => e.ContestantId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.ElectionPosition)
                    .WithMany()
                    .HasForeignKey(e => e.ElectionPositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RegisteredVoter>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.ElectionId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.VotingCode)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                b.HasIndex(e => new { e.StudentId, e.ElectionId })
                    .IsUnique();

                b.HasIndex(e => e.VotingCode)
                    .IsUnique();

                b.HasOne(e => e.Student)
                    .WithMany(e => e.RegisteredVoters)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.Election)
                    .WithMany(e => e.RegisteredVoters)
                    .HasForeignKey(e => e.ElectionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InvoiceStatus>(b =>
            {
                b.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Code)
                    .IsUnique();

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<PaymentStatus>(b =>
            {
                b.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Code)
                    .IsUnique();

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<PaymentGateway>(b =>
            {
                b.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Description)
                    .HasMaxLength(250);

                b.HasIndex(e => e.Code)
                    .IsUnique();

                b.HasIndex(e => e.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Invoice>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.PositionApplicationId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.InvoiceNumber)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                b.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                b.Property(e => e.PayerFirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.PayerLastName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.PayerEmail)
                    .HasMaxLength(256);

                b.Property(e => e.RegistrationNumber)
                    .HasMaxLength(50);

                b.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                b.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                b.HasIndex(e => e.PositionApplicationId)
                    .IsUnique();

                b.HasIndex(e => e.InvoiceNumber)
                    .IsUnique();

                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Invoices_Amount",
                    "[Amount] >= 0"));

                b.HasOne(e => e.PositionApplication)
                    .WithOne(e => e.Invoice)
                    .HasForeignKey<Invoice>(e => e.PositionApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.InvoiceStatus)
                    .WithMany(e => e.Invoices)
                    .HasForeignKey(e => e.InvoiceStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PaymentTransaction>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.InvoiceId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.PaymentReference)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                b.Property(e => e.ProviderReference)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                b.Property(e => e.CheckoutUrl)
                    .HasMaxLength(2000);

                b.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                b.Property(e => e.PayerFirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.PayerLastName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.PayerEmail)
                    .HasMaxLength(256);

                b.Property(e => e.RegistrationNumber)
                    .HasMaxLength(50);

                b.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                b.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                b.Property(e => e.FailureReason)
                    .HasMaxLength(500);

                b.HasIndex(e => e.PaymentReference)
                    .IsUnique();

                b.HasIndex(e => new
                {
                    e.PaymentGatewayId,
                    e.ProviderReference
                })
                .IsUnique()
                .HasFilter("[ProviderReference] IS NOT NULL");

                b.ToTable(t => t.HasCheckConstraint(
                    "CK_PaymentTransactions_Amount",
                    "[Amount] >= 0"));

                b.HasOne(e => e.Invoice)
                    .WithMany(e => e.PaymentTransactions)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.PaymentGateway)
                    .WithMany(e => e.PaymentTransactions)
                    .HasForeignKey(e => e.PaymentGatewayId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.PaymentStatus)
                    .WithMany(e => e.PaymentTransactions)
                    .HasForeignKey(e => e.PaymentStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IdempotencyRecord>(b =>
            {
                b.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                b.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                b.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                b.Property(e => e.Operation)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.RequestHash)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                b.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                b.Property(e => e.ResourceId)
                    .HasMaxLength(450);

                b.Property(e => e.Response)
                    .HasMaxLength(2000);

                b.HasIndex(e => new
                {
                    e.UserId,
                    e.Operation,
                    e.Key
                })
                .IsUnique();
            });
        }
    }
}
