using MassTransit;
using Play.Trading.Service.Data;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Activities
{
    public class CalculatePurchaseTotalActivity(TradingDbContext dbContext) : IStateMachineActivity<PurchaseState, PurchaseRequested>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, IBehavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Message;

            var item = await dbContext.CatalogItems.FindAsync(message.ItemId);

            if (item == null)
            {
                throw new UnknownItemException(message.ItemId);
            }

            context.Saga.PurchaseTotal = item.Price * message.Quantity;
            context.Saga.LastUpdated = DateTimeOffset.UtcNow;

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, IBehavior<PurchaseState, PurchaseRequested> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }
    }
}
