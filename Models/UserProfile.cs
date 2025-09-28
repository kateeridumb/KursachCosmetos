using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticShopAPI.Models;

public partial class UserProfile
{
    [Key]
    public int Id_Profile { get; set; }

    public int UserId { get; set; }

    public string? AddressPr { get; set; }

    public string? CityPr { get; set; }

    public string? PostalCodePr { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? Preferences { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
