using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class Order
{
    [Key]
    public int Id_Order { get; set; }

    public int UserID { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string StatusOr { get; set; } = null!;

    public string? DeliveryAddress { get; set; }

    public int? PromoID { get; set; }



}
