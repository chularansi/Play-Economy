using MassTransit;
using Play.Trading.Service.Data;
using Play.User.Contracts;

namespace Play.Trading.Service.Consumers
{
    public class UserUpdatedConsumer(TradingDbContext dbContext) : IConsumer<UserUpdated>
    {
        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;
            var user = await dbContext.Users.FindAsync(message.UserId);

            if (user == null) 
            {
                user = new Entities.User
                {
                    Id = message.UserId,
                    Gil = message.NewTotalGil
                };

                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                user.Gil = message.NewTotalGil;

                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
