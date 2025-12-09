using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Play.Common.PostgresDB
{
    public static class Extensions
    {
        public static IServiceCollection AddPostgresDB(this IServiceCollection services, DbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            services.AddScoped<IUnitOfWork, UnitOfWork>(uow => new UnitOfWork(dbContext));

            return services;
        }

        public static IServiceCollection AddPostgresRepository<T>(this IServiceCollection services) where T : class, IEntity
        {
            services.AddScoped(typeof(IRepository<T>), typeof(PostgresRepository<T>));
            return services;
        }
    }
}
