using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class Review
{
    public int IdReview { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string? CommentRe { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
