using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.StateMachines;
using System.Security.Claims;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController(
        IPublishEndpoint publishEndpoint, 
        IRequestClient<GetPurchaseState> purchaseClient,
        ILogger<PurchaseController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub");

            logger.LogInformation(
                "Received purchase of {Quantity} request of item {ItemId} from user {UserId}",
                purchaseDto.Quantity, 
                purchaseDto.ItemId, 
                userId);

            var message = new PurchaseRequested(
                Guid.Parse(userId),
                purchaseDto.ItemId.Value,
                purchaseDto.Quantity,
                purchaseDto.IdempotencyId.Value
            );

            await publishEndpoint.Publish(message);

            return AcceptedAtAction(nameof(GetStatusAsync), 
                new { purchaseDto.IdempotencyId }, 
                new { purchaseDto.IdempotencyId }
            );
        }

        [HttpGet("status/{idempotencyId}")]
        public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid idempotencyId)
        {
            var response = await purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(idempotencyId));

            var purchaseState = response.Message;

            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.PurchaseTotal,
                purchaseState.Quantity,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.Received,
                purchaseState.LastUpdated
            );

            return Ok(purchase);
        }
    }
}
