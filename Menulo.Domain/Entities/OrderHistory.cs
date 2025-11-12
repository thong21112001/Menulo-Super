namespace Menulo.Domain.Entities
{
    public class OrderHistory
    {
        public int OrderHistoryId { get; set; }
        public int OrderId { get; set; }
        public int RoundNo { get; set; }              // 1,2,3...
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Source { get; set; } = "customer"; // 'customer' | 'staff'
        public string? CreatedByUserId { get; set; }
        public string? Note { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
