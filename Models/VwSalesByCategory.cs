using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class VwSalesByCategory
{
    [Key]
    public string Категория { get; set; } = null!;

    public decimal? ОбщаяСуммаПродаж { get; set; }
}
