using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Play.Common.PostgresDB
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext dbContext;
        private Hashtable repositories;
        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        IRepository<T> IUnitOfWork.PostgresRepository<T>() where T : class
        {
            if (repositories == null)
                this.repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!this.repositories.ContainsKey(type))
            {
                var repositoryType = typeof(PostgresRepository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), this.dbContext);

                this.repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)this.repositories[type];
        }

        public async Task SaveChangesAsync()
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}
