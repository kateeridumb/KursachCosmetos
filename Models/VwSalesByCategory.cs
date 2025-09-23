using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class VwSalesByCategory
{
    public string Категория { get; set; } = null!;

    public decimal? ОбщаяСуммаПродаж { get; set; }
}
