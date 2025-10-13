

namespace CosmeticShopAPI.DTOs
{
    public class CategoryWithProductsDTO
    {
        public CategoryDTO Category { get; set; } = new CategoryDTO();
        public List<ProductDTO> FeaturedProducts { get; set; } = new List<ProductDTO>();
    }
}
