using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public int CategoryId { get; set; }

    public string NamePr { get; set; } = null!;

    public string? DescriptionPr { get; set; }

    public string? BrandPr { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
