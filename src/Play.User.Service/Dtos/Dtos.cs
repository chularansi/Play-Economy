using System.ComponentModel.DataAnnotations;

namespace Play.User.Service.Dtos
{
    public class Dtos
    {
        public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Username, decimal Gil);
        public record UpdateUserDto(
            [Range(0, 1000000)] decimal Gil
        );
    }
}
