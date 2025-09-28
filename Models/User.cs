using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.Models;

public partial class User
{
    [Key]
    public int Id_User { get; set; }

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

}
