using Menulo.Application.Common.Interfaces;
using Menulo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace Menulo.Infrastructure.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var repo = (IRepository<TEntity>)_repositories.GetOrAdd(
            typeof(TEntity),
            _ => Activator.CreateInstance(typeof(Repository<>).MakeGenericType(typeof(TEntity)), _context)!
            );

            return repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null) return;
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null) return;
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null) return;
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _context.DisposeAsync();
        }
    }
}
