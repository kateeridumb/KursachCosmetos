using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class VwUserOrder
{
    [Key]
    public int НомерЗаказа { get; set; }

    public string ФиоКлиента { get; set; } = null!;

    public string ЭлектроннаяПочта { get; set; } = null!;

    public DateTime ДатаЗаказа { get; set; }

    public decimal СуммаЗаказа { get; set; }

    public string СтатусЗаказа { get; set; } = null!;

    public string Промокод { get; set; } = null!;
}
