using System.Reflection.Metadata;

namespace Play.User.Service.Entities
{
    public class MessageIds(Guid messageId, Guid userId)
    {
        public Guid MessageId { get; set; } = messageId;
        public Guid UserId { get; set; } =  userId;
        public User User { get; set; }
    }
}
