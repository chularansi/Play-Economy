using Play.Common;

namespace Play.Trading.Service.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public decimal Gil { get; set; }
    }
}
