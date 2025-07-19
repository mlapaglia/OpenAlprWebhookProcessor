using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersContext _context;

        public UserRepository(UsersContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<User> GetFirstAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.CountAsync(predicate, cancellationToken);
        }

        public IQueryable<User> GetQueryable()
        {
            return _context.Users;
        }

        public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddRangeAsync(entities, cancellationToken);
        }

        public void Update(User entity)
        {
            _context.Users.Update(entity);
        }

        public void UpdateRange(IEnumerable<User> entities)
        {
            _context.Users.UpdateRange(entities);
        }

        public void Delete(User entity)
        {
            _context.Users.Remove(entity);
        }

        public void DeleteRange(IEnumerable<User> entities)
        {
            _context.Users.RemoveRange(entities);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Custom methods for User repository
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);
        }

        public async Task<List<User>> GetAllWithRefreshTokensAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .ToListAsync(cancellationToken);
        }

        public async Task<User> GetByIdWithRefreshTokensAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
        }

        public virtual async Task<List<TResult>> SelectAsync<TResult>(Expression<Func<User, TResult>> selector, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Select(selector).ToListAsync(cancellationToken);
        }
    }
} 