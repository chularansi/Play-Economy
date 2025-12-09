using MassTransit;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;

namespace Play.Inventory.Service.Consumers
{
    public class SubtractItemsConsumer(
        IRepository<InventoryItem> inventoryItemsRepository, 
        IRepository<CatalogItem> catalogItemsRepository) : IConsumer<SubtractItems>
    {
        public async Task Consume(ConsumeContext<SubtractItems> context)
        {
            var message = context.Message;
            var item = await catalogItemsRepository.GetAsync(message.CatalogItemId);

            if (item == null)
            {
                throw new UnknownItemException(message.CatalogItemId);
            }

            var inventoryItems = await inventoryItemsRepository.GetAsync(
                item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

            if (inventoryItems != null)
            {
                if (inventoryItems.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
                    return;
                }

                inventoryItems.Quantity -= message.Quantity;
                inventoryItems.MessageIds.Add(context.MessageId.Value);
                await inventoryItemsRepository.UpdateAsync(inventoryItems);

                await context.Publish(new InventoryItemUpdated(
                    inventoryItems.UserId,
                    inventoryItems.CatalogItemId,
                    inventoryItems.Quantity
                ));
            }

            await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
        }
    }
}
