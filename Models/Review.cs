using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class Review
{
    [Key]
    public int Id_Review { get; set; }

    public int ProductID { get; set; }

    public int UserID { get; set; }

    public int Rating { get; set; }

    public string? CommentRe { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
