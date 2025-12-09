using Microsoft.EntityFrameworkCore;
using Play.User.Service.Entities;

namespace Play.User.Service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Entities.User> Users { get; set; }
        public DbSet<MessageIds> MessageIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasColumnType("VARCHAR").HasMaxLength(100);
                entity.Property(e => e.LastName).HasColumnType("VARCHAR").HasMaxLength(100);
                entity.Property(e => e.LastName).HasColumnType("VARCHAR").HasMaxLength(100);
                entity.Property(e => e.Username).HasColumnType("VARCHAR").HasMaxLength(100);
                entity.Property(e => e.Gil).HasPrecision(8, 2);
            });

            modelBuilder.Entity<MessageIds>(entity =>
            {
                entity.HasKey(e => e.MessageId);
            });
        }
    }
}
