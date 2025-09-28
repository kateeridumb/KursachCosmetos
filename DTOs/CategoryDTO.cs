using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class CategoryDto
    {
        public int IdCategory { get; set; }
        public string NameCa { get; set; } = null!;
        public string? DescriptionCa { get; set; }
    }

}
