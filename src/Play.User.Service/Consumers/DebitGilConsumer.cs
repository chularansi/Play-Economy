using MassTransit;
using Microsoft.EntityFrameworkCore;
using Play.User.Contracts;
using Play.User.Service.Data;
using Play.User.Service.Exceptions;


namespace Play.User.Service.Consumers
{
    public class DebitGilConsumer : IConsumer<DebitGil>
    {
        private readonly ApplicationDbContext dbContext;

        public DebitGilConsumer(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<DebitGil> context)
        {
            var message = context.Message;
            var user = this.dbContext.Set<Entities.User>().Include(u => u.MessageIds).SingleOrDefault(u => u.Id == message.UserId);

            if (user == null)
            {
                throw new UnknownUserException(message.UserId);
            }

            if (this.dbContext.MessageIds.Where(m => m.MessageId == context.MessageId.Value && m.UserId == message.UserId).Count() > 0)
            {
                await context.Publish(new GilDebited(message.CorrelationId));
                return;
            }

            user.Gil -= message.Gil;

            if (user.Gil < 0)
            {
                throw new InsufficientFundsException(message.UserId, message.Gil);
            }

            await this.dbContext.MessageIds.AddAsync(new(context.MessageId.Value, user.Id));
            await this.dbContext.SaveChangesAsync();

            this.dbContext.Set<Entities.User>().Update(user);
            await this.dbContext.SaveChangesAsync();

            var gilDebitedTask = context.Publish(new GilDebited(message.CorrelationId));
            var userUpdatedTask = context.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            await Task.WhenAll(gilDebitedTask, userUpdatedTask);
        }
    }
}
