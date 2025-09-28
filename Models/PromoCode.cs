using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class PromoCode
{
    [Key]
    public int Id_Promo { get; set; }

    public string Code { get; set; } = null!;

    public int? DiscountPercent { get; set; }

    public int? MaxUsage { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

}
