namespace Menulo.Application.Features.Sales.Dtos
{
    /// <summary>
    /// DTO thanh gọn (lean DTO) để hiển thị danh sách Sale trên bảng (table/grid)
    /// Phiên bản này sử dụng { get; init; } để AutoMapper.ProjectTo
    /// có thể hoạt động (vì nó có default constructor).
    /// </summary>
    public sealed record SaleRowDto
    {
        // Thêm constructor rỗng (tùy chọn, C# tự thêm nếu không có constructor chính)
        public SaleRowDto() { }

        public string UserId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
    }
}
