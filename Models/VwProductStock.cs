using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class VwProductStock
{
    [Key]
    public int КодТовара { get; set; }

    public string НазваниеТовара { get; set; } = null!;

    public int НаСкладе { get; set; }

    public int Продано { get; set; }

    public int? Доступно { get; set; }
}
