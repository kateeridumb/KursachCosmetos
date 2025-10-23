namespace CosmeticShopWeb.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }
}
