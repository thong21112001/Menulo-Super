namespace Menulo.Application.Features.Sales.Dtos
{
    /// <summary>
    /// DTO thanh gọn (lean DTO) để hiển thị danh sách Sale trên bảng (table)
    /// </summary>
    public sealed record SaleRowDto
    (
        string UserId,
        string FullName,
        string Username,
        string Email,
        string PhoneNumber
    );
}
