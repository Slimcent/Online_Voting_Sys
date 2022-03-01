using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Context
{
    public class VotingDbContext : IdentityDbContext<User, Role, string>
    {
        public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is ITracker trackable)
                {
                    var now = DateTime.UtcNow;
                    //var user = GetCurrentUser();
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedAt = now;
                            //trackable.UpdatedBy = user;
                            break;

                        case EntityState.Added:
                            trackable.CreatedAt = now;
                            trackable.UpdatedAt = now;
                            //trackable.CreatedBy = user;
                            //trackable.UpdatedBy = user;
                            break;
                    }
                }
            }
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Contestant> Contestans { get; set; }
        public DbSet<Position> Positions { get; set; }
    }
}
