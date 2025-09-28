using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class ImageDTO
    {
        public int IdImage { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? DescriptionImg { get; set; }
    }
}
