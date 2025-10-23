using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class ImageDTO
    {
        public int ID_Image { get; set; }
        public int ProductID { get; set; }
        public string ImageURL { get; set; } = null!;
        public string? DescriptionIMG { get; set; }
    }
}
