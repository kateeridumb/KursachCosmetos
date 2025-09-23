using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class PromoCode
{
    public int IdPromo { get; set; }

    public string Code { get; set; } = null!;

    public int? DiscountPercent { get; set; }

    public int? MaxUsage { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
