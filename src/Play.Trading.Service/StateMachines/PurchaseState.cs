using MassTransit;

namespace Play.Trading.Service.StateMachines
{
    public class PurchaseState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get ; set ; }
        public string CurrentState { get; set; }
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset Received { get; set; }
        public Decimal PurchaseTotal { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string ErrorMessage { get; set; }
    }
}
