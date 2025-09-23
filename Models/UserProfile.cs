using System;
using System.Collections.Generic;

namespace CosmeticShopAPI.Models;

public partial class UserProfile
{
    public int IdProfile { get; set; }

    public int UserId { get; set; }

    public string? AddressPr { get; set; }

    public string? CityPr { get; set; }

    public string? PostalCodePr { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? Preferences { get; set; }

    public virtual User User { get; set; } = null!;
}
