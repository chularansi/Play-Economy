using MassTransit;
using Play.Catalog.Contracts;
using Play.Trading.Service.Data;

namespace Play.Trading.Service.Consumers
{
    public class CatalogItemDeletedConsumer(TradingDbContext dbContext) : IConsumer<CatalogItemDeleted>
    {
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;

            var item = await dbContext.CatalogItems.FindAsync(message.ItemId);

            if (item != null)
            {
                dbContext.CatalogItems.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
