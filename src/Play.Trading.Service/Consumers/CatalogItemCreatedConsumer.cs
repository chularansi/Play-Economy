using MassTransit;
using Play.Catalog.Contracts;
using Play.Trading.Service.Data;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Consumers
{
    public class CatalogItemCreatedConsumer(TradingDbContext dbContext) : IConsumer<CatalogItemCreated>
    {
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;

            var item = await dbContext.CatalogItems.FindAsync(message.ItemId);

            if (item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description,
                    Price = message.Price,
                };
                
                await dbContext.CatalogItems.AddAsync(item);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
