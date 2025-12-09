using System.Linq.Expressions;

namespace Play.Common
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task CreateAsync(T entity);
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<T> GetAsync(Guid id);
        Task<T> GetAsync(Expression<Func<T, bool>> filter);
        Task RemoveAsync(Guid id);
        Task UpdateAsync(T entity);

        // Can use this for custom query if other methods are not satisfied
        //IQueryable<T> GetQueryable();
        //Task AddRangeAsync(IEnumerable<TEntity> entities);
        //void RemoveRange(IEnumerable<TEntity> entities);
    }
}