using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string RoleUs { get; set; } = null!;

    public DateOnly DateRegistered { get; set; }

    public string StatusUs { get; set; } = null!;

    public int Points { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual UserProfile? UserProfile { get; set; }
}
