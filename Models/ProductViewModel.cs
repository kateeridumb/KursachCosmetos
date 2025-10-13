namespace CosmeticShopWeb.Models
{
    public class ProductViewModel
    {
        public int IdProduct { get; set; }
        public int CategoryId { get; set; }
        public string NamePr { get; set; } = string.Empty;
        public string DescriptionPr { get; set; } = string.Empty;
        public string BrandPr { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        public string MainImageUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<ProductViewModel> RelatedProducts { get; set; } = new List<ProductViewModel>();

        public string CategoryName => CategoryId switch
        {
            1 => "Декоративная косметика",
            2 => "Уход за кожей",
            3 => "Парфюмерия",
            4 => "Уход за волосами",
            5 => "Уход за телом",
            6 => "Люкс косметика",
            _ => "Все товары"
        };
        public string CategoryIcon => CategoryId switch
        {
            1 => "💄",
            2 => "✨",
            3 => "🌺",
            4 => "💆‍♀️",
            5 => "🛁",
            6 => "🌟",
            _ => "📦"
        };

        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }

        public string FormattedPrice => Price.ToString("N2") + "₽";
        public string StockStatus => StockQuantity switch
        {
            > 10 => "В наличии",
            > 0 => "Мало",
            _ => "Нет в наличии"
        };
        public string StockStatusClass => StockStatus switch
        {
            "В наличии" => "in-stock",
            "Мало" => "low-stock",
            _ => "out-of-stock"
        };
        public bool HasReviews => Reviews?.Any() == true;
    }
}
