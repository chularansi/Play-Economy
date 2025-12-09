using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController(
        IRepository<InventoryItem> inventoryItemsRepository, 
        IRepository<CatalogItem> catalogItemsRepository,
        IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetUserItemsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (Guid.Parse(currentUserId) != userId)
            {
                if (!User.IsInRole("Admin"))
                {
                    return Forbid();
                }
            }

            var inventoryItemsEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
            var itemIds = inventoryItemsEntities.Select(item => item.CatalogItemId);
            var catalogItemsEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemsDto = inventoryItemsEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemsEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId); 
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemsDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostGrantItemsAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItems = await inventoryItemsRepository.GetAsync(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItems == null)
            {
                inventoryItems = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow,
                };

                await inventoryItemsRepository.CreateAsync(inventoryItems);
            }
            else
            {
                inventoryItems.Quantity += grantItemsDto.Quantity;
                await inventoryItemsRepository.UpdateAsync(inventoryItems);
            }

            await publishEndpoint.Publish(new InventoryItemUpdated(
                inventoryItems.UserId,
                inventoryItems.CatalogItemId,
                inventoryItems.Quantity
            ));

            return Ok();
        }
    }
}
