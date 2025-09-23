using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class Image
{
    public int IdImage { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? DescriptionImg { get; set; }

    public virtual Product Product { get; set; } = null!;
}
