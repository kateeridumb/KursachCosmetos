using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class UserProfileDTO
    {
        public int IdProfile { get; set; }
        public int UserId { get; set; }
        public string? AddressPr { get; set; }
        public string? CityPr { get; set; }
        public string? PostalCodePr { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Preferences { get; set; }
    }
}
