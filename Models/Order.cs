using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string StatusOr { get; set; } = null!;

    public string? DeliveryAddress { get; set; }

    public int? PromoId { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual PromoCode? Promo { get; set; }

    public virtual User User { get; set; } = null!;
}
