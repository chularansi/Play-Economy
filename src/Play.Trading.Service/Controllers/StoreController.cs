using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play.Trading.Service.Data;
using System.Security.Claims;

namespace Play.Trading.Service.Controllers
{
    [Route("store")]
    [ApiController]
    [Authorize]
    public class StoreController(TradingDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            var userId = User.FindFirstValue("sub");

            var catalogItems = await dbContext.CatalogItems.ToListAsync();
            var inventoryItems = dbContext.InventoryItems
                .Where(item => item.UserId == Guid.Parse(userId));
                //.FirstOrDefaultAsync();
            var user = await dbContext.Users.FindAsync(Guid.Parse(userId));

            var storeDto = new StoreDto(
                catalogItems.Select(catalogItem => 
                    new StoreItemDto(
                        catalogItem.Id,
                        catalogItem.Name,
                        catalogItem.Description,
                        catalogItem.Price,
                        inventoryItems.FirstOrDefault(
                            inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
                    )
                ),
                user?.Gil ?? 0
            );

            return Ok(storeDto);
        }
    }
}
