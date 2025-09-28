using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticShopAPI.Models;

public partial class AuditLog
{
    [Key]
    public int Id_Log { get; set; }

    public int? UserID { get; set; }

    public string? UserName { get; set; }

    public string TableName { get; set; } = null!;

    public string ActionType { get; set; } = null!;

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    public DateTime TimestampMl { get; set; }

    [ForeignKey(nameof(UserID))]
    public virtual User? User { get; set; }
}

