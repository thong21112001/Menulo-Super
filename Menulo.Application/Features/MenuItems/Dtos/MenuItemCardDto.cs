namespace Menulo.Application.Features.MenuItems.Dtos
{
    /// <summary>
    /// DTO "thẻ" món ăn, chỉ chứa thông tin cần thiết để hiển thị menu
    /// </summary>
    public sealed record MenuItemCardDto
    (
        int ItemId,
        string ItemName,
        decimal? Price,
        string? ImageUrl
    );
}
