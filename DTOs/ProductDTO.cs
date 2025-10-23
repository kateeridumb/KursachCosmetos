namespace CosmeticShopAPI.DTOs
{
    public class ProductDTO
    {
        public int IdProduct { get; set; }
        public int CategoryId { get; set; }
        public string NamePr { get; set; }
        public string DescriptionPr { get; set; }
        public string BrandPr { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string ImageUrl { get; set; }
    }
}