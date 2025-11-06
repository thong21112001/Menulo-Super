namespace Menulo.Application.Features.MenuItems.Dtos
{
    /// <summary>
    /// DTO chứa một nhóm danh mục và các món ăn thuộc nhóm đó
    /// </summary>
    public sealed record MenuCategoryGroupDto
    (
        int CategoryId,
        string CategoryName,
        int CategoryPriority,
        List<MenuItemCardDto> Items // Chứa danh sách các món ăn
    );
}
