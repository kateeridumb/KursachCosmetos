using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class OrderDetail
{
    [Key]
    public int Id_OrderDetail { get; set; }

    public int OrderID { get; set; }

    public int ProductID { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
