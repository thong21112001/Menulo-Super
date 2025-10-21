namespace Menulo.Application.Common.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        //Cung cấp khả năng xóa nhiều bản ghi
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        //Cung cấp khả năng truy vấn linh hoạt
        IQueryable<TEntity> GetQueryable();

        // Phương thức để lấy dữ liệu có phân trang và tùy chỉnh hình dạng dữ liệu
        Task<(IReadOnlyList<TEntity> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? shape = null,
            CancellationToken ct = default);

        // Phương thức để thêm nhiều bản ghi cùng lúc
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}
