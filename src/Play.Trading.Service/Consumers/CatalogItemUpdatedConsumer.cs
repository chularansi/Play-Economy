using MassTransit;
using Play.Catalog.Contracts;
using Play.Trading.Service.Data;

namespace Play.Trading.Service.Consumers
{
    public class CatalogItemUpdatedConsumer(TradingDbContext dbContext) : IConsumer<CatalogItemUpdated>
    {
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;

            var item = await dbContext.CatalogItems.FindAsync(message.ItemId);

            if (item != null)
            {
                item.Name = message.Name;
                item.Description = message.Description;
                item.Price = message.Price;

                dbContext.CatalogItems.Update(item);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
