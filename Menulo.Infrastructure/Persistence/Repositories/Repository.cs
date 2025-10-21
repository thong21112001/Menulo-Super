using Menulo.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Infrastructure.Persistence.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _db;

        public Repository(AppDbContext db)
        {
            _db = db;
        }


        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _db.Set<TEntity>().AddAsync(entity, cancellationToken);
            // Chưa SaveChanges() ở đây để tuân thủ Unit of Work
            return entity;
        }

        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _db.Set<TEntity>().AddRange(entities);
            // Không SaveChanges ở đây để tuân thủ UoW
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _db.Set<TEntity>().Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _db.Set<TEntity>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _db.Set<TEntity>().RemoveRange(entities);
            return Task.CompletedTask;
        }

        public IQueryable<TEntity> GetQueryable()
        {
            return _db.Set<TEntity>();
        }

        public async Task<(IReadOnlyList<TEntity>, int)> GetPagedAsync(
            int page, int pageSize,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? shape = null,
            CancellationToken ct = default)
        {
            var query = _db.Set<TEntity>().AsQueryable();
            if (shape != null) query = shape(query);

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }
    }
}
