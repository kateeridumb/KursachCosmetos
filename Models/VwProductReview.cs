using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class VwProductReview
{
    public int НомерОтзыва { get; set; }

    public string? АвторОтзыва { get; set; }

    public string Товар { get; set; } = null!;

    public int Оценка { get; set; }

    public string Комментарий { get; set; } = null!;

    public DateTime ДатаОтзыва { get; set; }
}
