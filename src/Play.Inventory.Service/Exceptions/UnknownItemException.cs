namespace Play.Inventory.Service.Exceptions
{
    [Serializable]
    internal class UnknownItemException(Guid ItemId) : Exception($"Unknown item '{ItemId}'")
    {
        public Guid ItemId { get; } = ItemId;

        //public UnknownItemException(Guid ItemId) : base($"Unknown item '{ItemId}'")
        //{
        //    this.ItemId = ItemId;
        //}
    }
}