namespace CosmeticShopWeb.Models
{
    public class CategoryViewModel
    {
        public int IdCategory { get; set; }
        public string NameCa { get; set; } = string.Empty;
        public string DescriptionCa { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}