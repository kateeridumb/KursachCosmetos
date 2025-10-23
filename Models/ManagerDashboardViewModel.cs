namespace CosmeticShopWeb.Models
{
    public class ManagerDashboardViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public SalesChartData SalesData { get; set; } = new SalesChartData();
        public StockChartData StockData { get; set; } = new StockChartData();
        public CategoryChartData CategoryData { get; set; } = new CategoryChartData();
    }

    public class SalesChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
    }

    public class StockChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Values { get; set; } = new List<int>();
    }

    public class CategoryChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Values { get; set; } = new List<int>();
    }
}
