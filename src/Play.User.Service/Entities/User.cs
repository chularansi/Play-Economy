using Play.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Play.User.Service.Entities
{
    [Table("users")]
    public class User : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("firstname")]
        public string FirstName { get; set; }
        [Column("lastname")]
        public string LastName { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("gil")]
        public decimal Gil { get; set; }
        public ICollection<MessageIds> MessageIds { get; set; } = new HashSet<MessageIds>();
    }
}
