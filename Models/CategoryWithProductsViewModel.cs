using CosmeticShopWeb.Models;

public class CategoryWithProductsViewModel
{
    public CategoryViewModel Category { get; set; } = new CategoryViewModel();
    public List<ProductViewModel> FeaturedProducts { get; set; } = new List<ProductViewModel>();
}