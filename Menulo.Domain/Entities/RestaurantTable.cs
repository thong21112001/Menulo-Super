namespace Menulo.Domain.Entities
{
    public class RestaurantTable
    {
        public int TableId { get; set; }

        public int RestaurantId { get; set; }

        public string TableCode { get; set; } = null!;

        public string? Description { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<ItemsTmp> ItemsTmps { get; set; } = new List<ItemsTmp>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
