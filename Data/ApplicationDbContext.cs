using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FileDigitilizationSystem.Models;

namespace FileDigitilizationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define additional DbSets for other entities here if needed.
       
        public DbSet<FileRecord> FileRecords { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FileRequest> FileRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FileRecord>()
                .HasOne(fr => fr.CreatedBy)
                .WithMany()
                .HasForeignKey(fr => fr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FileRequest>()
                .HasOne(dr => dr.Requester)
                .WithMany()
                .HasForeignKey(dr => dr.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}

