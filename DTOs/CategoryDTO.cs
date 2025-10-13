using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class CategoryDTO
    {
        public int IdCategory { get; set; }
        public string NameCa { get; set; } = string.Empty;
        public string DescriptionCa { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}
