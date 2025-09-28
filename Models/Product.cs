using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class Product
{
    [Key]
    public int Id_Product { get; set; }

    public int CategoryID { get; set; }

    [Required]
    public string NamePr { get; set; } = null!;

    public string? DescriptionPr { get; set; }

    public string? BrandPr { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; }


}
