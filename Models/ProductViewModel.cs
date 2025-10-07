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

        public string FormattedPrice => Price.ToString("C");
        public string StockStatus
        {
            get
            {
                if (!IsAvailable || StockQuantity == 0) return "Нет в наличии";
                if (StockQuantity < 10) return "Мало";
                return "В наличии";
            }
        }
        public string StockStatusClass
        {
            get
            {
                if (!IsAvailable || StockQuantity == 0) return "out-of-stock";
                if (StockQuantity < 10) return "low-stock";
                return "in-stock";
            }
        }
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

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
