using CosmeticShopWeb.Models;

public class CategoryDetailsViewModel
{
    public CategoryViewModel Category { get; set; } = new CategoryViewModel();
    public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
}