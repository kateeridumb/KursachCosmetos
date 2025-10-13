namespace CosmeticShopWeb.Models
{

    public class OrderCreateResponse
    {
        public int Id_Order { get; set; }
        public int UserID { get; set; }
        public string OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusOr { get; set; }
        public string DeliveryAddress { get; set; }
        public int? PromoID { get; set; }
    }
}
