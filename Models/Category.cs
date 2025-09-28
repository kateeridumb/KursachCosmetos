using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class Category
{
    [Key]
    public int Id_Category { get; set; }

    public string NameCa { get; set; } = null!;

    public string? DescriptionCa { get; set; }

}
