using System.Data.Entity;
using Diploma.Models;

namespace Diploma.Data
{
    public class DefectDbContext : DbContext
    {
        public DefectDbContext() : base("name=DefectDbContext")
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Object> Objects { get; set; }
        public DbSet<Premise> Premises { get; set; }
        public DbSet<Defect> Defects { get; set; }
        public DbSet<DefectStatusHistory> DefectStatusHistory { get; set; }
        public DbSet<DefectComment> DefectComments { get; set; }
        public DbSet<DefectMedia> DefectMedia { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedDefects)
                .WithRequired(d => d.CreatedByUser)
                .HasForeignKey(d => d.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedDefects)
                .WithOptional(d => d.AssignedToUser)
                .HasForeignKey(d => d.AssignedToUserId)
                .WillCascadeOnDelete(false);

            // Конфигурация Defect
            modelBuilder.Entity<Defect>()
                .HasMany(d => d.Comments)
                .WithRequired(c => c.Defect)
                .HasForeignKey(c => c.DefectId);

            modelBuilder.Entity<Defect>()
                .HasMany(d => d.MediaFiles)
                .WithRequired(m => m.Defect)
                .HasForeignKey(m => m.DefectId);

            modelBuilder.Entity<Defect>()
                .HasMany(d => d.StatusHistory)
                .WithRequired(h => h.Defect)
                .HasForeignKey(h => h.DefectId);
        }
    }
}