namespace CosmeticShopAPI.DTOs
{
    public class ProductDTO
    {
        public int IdProduct { get; set; }
        public int CategoryId { get; set; }
        public string NamePr { get; set; } = string.Empty;
        public string DescriptionPr { get; set; } = string.Empty;
        public string BrandPr { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
    }
}