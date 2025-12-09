using static Play.User.Service.Dtos.Dtos;

namespace Play.User.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this Entities.User user)
        {
            return new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Username, user.Gil);
        }
    }
}
