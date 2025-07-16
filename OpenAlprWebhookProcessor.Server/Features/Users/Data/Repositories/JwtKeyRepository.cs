using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data.Repositories
{
    public class JwtKeyRepository : IJwtKeyRepository
    {
        private readonly UsersContext _context;

        public JwtKeyRepository(UsersContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<JwtKey> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<JwtKey>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<JwtKey>> FindAsync(Expression<Func<JwtKey, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<JwtKey> FirstOrDefaultAsync(Expression<Func<JwtKey, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<JwtKey, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<JwtKey, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.CountAsync(predicate, cancellationToken);
        }

        public IQueryable<JwtKey> GetQueryable()
        {
            return _context.JwtKeys;
        }

        public async Task AddAsync(JwtKey entity, CancellationToken cancellationToken = default)
        {
            await _context.JwtKeys.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<JwtKey> entities, CancellationToken cancellationToken = default)
        {
            await _context.JwtKeys.AddRangeAsync(entities, cancellationToken);
        }

        public void Update(JwtKey entity)
        {
            _context.JwtKeys.Update(entity);
        }

        public void UpdateRange(IEnumerable<JwtKey> entities)
        {
            _context.JwtKeys.UpdateRange(entities);
        }

        public void Delete(JwtKey entity)
        {
            _context.JwtKeys.Remove(entity);
        }

        public void DeleteRange(IEnumerable<JwtKey> entities)
        {
            _context.JwtKeys.RemoveRange(entities);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Custom methods for JwtKey repository
        public async Task<JwtKey> GetFirstJwtKeyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.JwtKeys.FirstOrDefaultAsync(cancellationToken);
        }
    }
} 