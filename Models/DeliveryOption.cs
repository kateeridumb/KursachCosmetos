namespace CosmeticShopWeb.Models
{
    public class DeliveryOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public int EstimatedDays { get; set; }
    }
}
