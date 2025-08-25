namespace Menulo.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Lấy ra một repository cho một entity cụ thể
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // Lưu các thay đổi vào cơ sở dữ liệu
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        //Các phương thức quản lý Transaction
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
