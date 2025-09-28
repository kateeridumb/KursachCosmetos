using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class PromoCodeDTO
    {
        public int IdPromo { get; set; }
        public string Code { get; set; } = null!;
        public int? DiscountPercent { get; set; }
        public int? MaxUsage { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }

}
