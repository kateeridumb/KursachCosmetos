using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class VwUserOrder
{
    public int НомерЗаказа { get; set; }

    public string ФиоКлиента { get; set; } = null!;

    public string ЭлектроннаяПочта { get; set; } = null!;

    public DateTime ДатаЗаказа { get; set; }

    public decimal СуммаЗаказа { get; set; }

    public string СтатусЗаказа { get; set; } = null!;

    public string Промокод { get; set; } = null!;
}
