namespace CosmeticShopWeb.Models
{
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public int SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
    }
}