using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        //private readonly ItemsRepository itemsRepository = new();
        //private static readonly List<ItemDto> items =
        //[
        //    new ItemDto(Guid.NewGuid(), "Potion", "Restores a small amount of HP", 5, DateTimeOffset.UtcNow),
        //    new ItemDto(Guid.NewGuid(), "Antidote", "Cures poisen", 7, DateTimeOffset.UtcNow),
        //    new ItemDto(Guid.NewGuid(), "Bronze sword", "Deals a small amount of damage", 20, DateTimeOffset.UtcNow),
        //];

        [HttpGet]
        [Authorize(Policy = "read_access")]
        public async Task<IEnumerable<ItemDto>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());

            return items;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "read_access")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        [HttpPost]
        [Authorize(Policy = "write_access")]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item {
                Name = createItemDto.Name, 
                Description = createItemDto.Description,
                Price = createItemDto.Price, 
                CreatedDate = DateTimeOffset.UtcNow 
            };

            await itemsRepository.CreateAsync(item);

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description, item.Price));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "write_access")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id);

            if (existingItem == null) 
            { 
                return BadRequest();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);

            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description, existingItem.Price));

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "write_access")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return BadRequest();
            }

            await itemsRepository.RemoveAsync(id);

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}
