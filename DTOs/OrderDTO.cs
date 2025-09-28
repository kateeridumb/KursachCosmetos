using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
