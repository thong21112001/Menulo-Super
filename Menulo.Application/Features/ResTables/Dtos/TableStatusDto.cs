namespace Menulo.Application.Features.ResTables.Dtos
{
    /// <summary>
    /// DTO chứa trạng thái realtime của bàn
    /// </summary>
    public sealed record TableStatusDto
    (
        int TableId,
        string Description,
        bool HasOrder,           // Có đơn chính thức (Pending)
        int TotalQuantity,       // Tổng món (chính thức)
        decimal TotalAmount,     // Tổng tiền (chính thức, đã giảm giá)
        bool HasPendingOrder,    // Có đơn tạm (khách đang chọn)
        int PendingTotalQuantity // Tổng món (tạm)
    );
}
