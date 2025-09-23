using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class VwProductStock
{
    public int КодТовара { get; set; }

    public string НазваниеТовара { get; set; } = null!;

    public int НаСкладе { get; set; }

    public int Продано { get; set; }

    public int? Доступно { get; set; }
}
