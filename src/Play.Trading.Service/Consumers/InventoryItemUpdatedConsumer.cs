using MassTransit;
using Microsoft.EntityFrameworkCore;
using Play.Inventory.Contracts;
using Play.Trading.Service.Data;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Consumers
{
    public class InventoryItemUpdatedConsumer(TradingDbContext dbContext) : IConsumer<InventoryItemUpdated>
    {
        public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
        {
            var message = context.Message;

            var inventoryItem = await dbContext.InventoryItems
                .Where(i => i.UserId == message.UserId && i.CatalogItemId == message.CatalogItemId)
                .FirstOrDefaultAsync();

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem { 
                    UserId = message.UserId, 
                    CatalogItemId = message.CatalogItemId,
                    Quantity = message.NewTotalQuantity
                };

                await dbContext.InventoryItems.AddAsync(inventoryItem);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                inventoryItem.Quantity = message.NewTotalQuantity;
                dbContext.InventoryItems.Update(inventoryItem);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
