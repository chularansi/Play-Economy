using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Play.Common.Settings;
using System.Linq.Expressions;

namespace Play.Common.PostgresDB
{
    //where T : class enforces that T must be a reference type(a class). This excludes structs.
    //where T : IMyInterface enforces that T must implement the IMyInterface interface.

    //When combining multiple constraints in a where clause, they are separated by commas.
    //The class constraint always comes first if present, followed by any interface constraints. 
    //This ensures that any type used for T will satisfy both conditions: it will be a class, 
    //and it will provide the functionality defined by IMyInterface.

    public class PostgresRepository<T> : IRepository<T> where T : class, IEntity
    {
        //private readonly IOptions<PostgresDBSettings> options;
        private readonly DbContext context;

        //public PostgresRepository(IOptions<PostgresDBSettings> options)
        public PostgresRepository(DbContext context)
        {
            //this.options = options;
            //this.context = this.options.Value.AppDbContext;
            this.context = context;
        }

        public async Task CreateAsync(T entity)
        {
            await this.context.Set<T>().AddAsync(entity);
            await this.context.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await this.context.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await this.context.Set<T>().Where(filter).ToListAsync();
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await this.context.Set<T>().FindAsync(id);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await this.context.Set<T>().Where(filter).FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var entity = await this.context.Set<T>().FindAsync(id);
            this.context.Set<T>().Remove(entity);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            this.context.Set<T>().Update(entity);
            await this.context.SaveChangesAsync();
        }

        //public IQueryable<T> GetQueryable() { return context.Set<T>(); }
    }
}
