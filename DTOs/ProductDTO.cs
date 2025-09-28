using CosmeticShopAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShop.Core.DTOs
{
    public class ProductDTO
    {
        public int IdProduct { get; set; }
        public int CategoryId { get; set; }
        public string NamePr { get; set; } = null!;
        public string? DescriptionPr { get; set; }
        public string? BrandPr { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
    }
}
