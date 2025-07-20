using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T?> GetFirstAsync(CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<List<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default);

        IQueryable<T> GetQueryable();

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);

        void DeleteRange(IEnumerable<T> entities);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 