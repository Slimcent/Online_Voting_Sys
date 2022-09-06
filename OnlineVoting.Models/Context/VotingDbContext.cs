using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using System;

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
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<Staff> StaffProfile { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            //modelBuilder.Entity<Department>(entity =>
            //{
            //    entity.ToTable("DEPARTMENT");

            //    entity.Property(e => e.Id)
            //        .HasMaxLength(10)
            //        .IsUnicode(false)
            //        .HasColumnName("DEPARTMENT_ID");

            //    entity.Property(e => e.Activated).HasColumnName("ACTIVATED");

            //    entity.Property(e => e.Name)
            //        .IsRequired()
            //        .HasMaxLength(50)
            //        .IsUnicode(false)
            //        .HasColumnName("DEPARTMENT_NAME");

                
            //    entity.Property(e => e.FacultyId)
            //        .IsRequired()
            //        .HasMaxLength(5)
            //        .HasColumnName("FACULTY_ID")
            //        .IsFixedLength(false);

                
            //    entity.HasOne(d => d.Faculty)
            //        .WithMany(p => p.Departments)
            //        .HasForeignKey(d => d.FacultyId)
            //        .OnDelete(DeleteBehavior.ClientSetNull)
            //        .HasConstraintName("FK_DEPARTMENT_FACULTY");
            //});
                       
            //modelBuilder.Entity<Faculty>(entity =>
            //{
            //    entity.ToTable("FACULTY");

            //    entity.Property(e => e.Id)
            //        .HasMaxLength(5)
            //        .HasColumnName("FACULTY_ID")
            //        .IsFixedLength(false);

            //    entity.Property(e => e.Activated).HasColumnName("ACTIVATED");

            //    entity.Property(e => e.Name)
            //        .IsRequired()
            //        .HasMaxLength(50)
            //        .IsUnicode(false)
            //        .HasColumnName("FACULTY_NAME");
            //});                   

            //OnModelCreatingPartial(modelBuilder);
        }

        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
