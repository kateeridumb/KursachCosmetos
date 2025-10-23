namespace CosmeticShopWeb.Models
{
    public class ApiOrderResponse
    {
        public int IdOrder { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusOr { get; set; }
        public string DeliveryAddress { get; set; }
        public int? PromoId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public List<ApiOrderDetailResponse> OrderDetails { get; set; } = new List<ApiOrderDetailResponse>();
    }
}
