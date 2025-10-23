using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.DTOs
{
    public class OrderDTO
    {
        public int IdOrder { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusOr { get; set; } = null!;
        public string? DeliveryAddress { get; set; }
        public int? PromoId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string DeliveryMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string Comment { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }
}