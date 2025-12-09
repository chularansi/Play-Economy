namespace Play.User.Service.Exceptions
{
    [Serializable]
    internal class InsufficientFundsException(Guid UserId, decimal Gil) : 
        Exception($"Not enough Gil to debit '{Gil}' from user '{UserId}'")
    {
        public Guid UserId { get; } = UserId;
        public decimal Gil { get; } = Gil;
    }
}