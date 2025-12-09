using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Data
{
    public class TradingSagaDbContext : SagaDbContext
    {
        public TradingSagaDbContext(DbContextOptions<TradingSagaDbContext> options) : base(options) {}

        //public DbSet<PurchaseState> PurchaseStates { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<PurchaseState>(entity =>
        //    {
        //        entity.HasKey(e => e.CorrelationId);
        //        entity.Property(e => e.CurrentState).HasMaxLength(50);
        //        entity.Property(e => e.PurchaseTotal).HasPrecision(8, 2);
        //    });

        //    modelBuilder.Entity<CatalogItem>(entity =>
        //    {
        //        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        //        entity.Property(e => e.Description).HasMaxLength(250);
        //        entity.Property(e => e.Price).HasPrecision(8, 2);
        //    });
        //}

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new PurchaseStateMap();
            }
        }
    }

    public class PurchaseStateMap : SagaClassMap<PurchaseState>
    {
        protected override void Configure(EntityTypeBuilder<PurchaseState> entity, ModelBuilder model)
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(50);
            entity.Property(x => x.PurchaseTotal).HasPrecision(8, 2);
        }
    }
}
