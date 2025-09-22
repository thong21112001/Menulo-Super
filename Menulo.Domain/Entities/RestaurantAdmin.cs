namespace Menulo.Domain.Entities
{
    public class RestaurantAdmin
    {
        public string UserId { get; set; } = default!;

        public int? RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }
    }
}
