using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class AuditLog
{
    public int IdLog { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string TableName { get; set; } = null!;

    public string ActionType { get; set; } = null!;

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    public DateTime TimestampMl { get; set; }

    public virtual User? User { get; set; }
}
