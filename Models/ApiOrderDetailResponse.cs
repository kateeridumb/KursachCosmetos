namespace CosmeticShopWeb.Models
{
    public class ApiOrderDetailResponse
    {
        public int IdOrderDetail { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
