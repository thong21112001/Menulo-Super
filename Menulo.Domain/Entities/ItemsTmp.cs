namespace Menulo.Domain.Entities
{
    public class ItemsTmp
    {
        public int ItemsTmpId { get; set; }

        public int TableId { get; set; }

        public int ItemId { get; set; }

        public int Quantity { get; set; }

        public MenuItem Item { get; set; } = null!;

        public RestaurantTable Table { get; set; } = null!;
    }
}
