using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Play.Common.PostgresDB
{
    public interface IUnitOfWork
    {
        IRepository<T> PostgresRepository<T>() where T : class, IEntity;
        Task SaveChangesAsync();
    }
}
