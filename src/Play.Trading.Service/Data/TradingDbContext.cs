using Microsoft.EntityFrameworkCore;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Data
{
    public class TradingDbContext: DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Entities.User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogItem>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.Price).HasPrecision(8, 2);
                entity.HasData(SeedCatalogItems);
            });

            modelBuilder.Entity<Entities.User>(entity =>
            {
                entity.Property(e => e.Gil).HasPrecision(8, 2);
            });
        }

        private static readonly CatalogItem[] SeedCatalogItems =
        {
            new() {Id = Guid.Parse("92453fe9-daba-47f8-8e81-257a7dd1aea7"), Name = "Potion", Description = "Restores a small amount of HP", Price = 6},
            new() {Id = Guid.Parse("7e73026b-52a9-4cd8-9b8f-6a24a6b504f2"), Name = "Antidote", Description = "Cures poisen", Price = 8},
            new() {Id = Guid.Parse("802f84f0-8e91-41fd-b72d-46f8ca40bb2e"), Name = "Bronze sword", Description = "Deals a small amount of damage", Price = 10}

        };
    }
}
