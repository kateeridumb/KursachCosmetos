using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class Category
{
    public int IdCategory { get; set; }

    public string NameCa { get; set; } = null!;

    public string? DescriptionCa { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
