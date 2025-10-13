using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticShopAPI.Models;
using CosmeticShopAPI.DTOs;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public CategoriesController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDTO>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryDTO
                    {
                        IdCategory = c.Id_Category,
                        NameCa = c.NameCa,
                        DescriptionCa = c.DescriptionCa,
                        ProductCount = _context.Products.Count(p => p.CategoryID == c.Id_Category && p.IsAvailable)
                    })
                    .OrderBy(c => c.IdCategory)
                    .ToListAsync();

                foreach (var cat in categories)
                {
                    cat.Icon = GetCategoryIcon(cat.IdCategory);
                }

                categories.Insert(0, new CategoryDTO
                {
                    IdCategory = 0,
                    NameCa = "Все товары",
                    DescriptionCa = "Все товары магазина",
                    Icon = "📦",
                    ProductCount = await _context.Products.CountAsync(p => p.IsAvailable)
                });

                return Ok(new ApiResponse<IEnumerable<CategoryDTO>>
                {
                    Success = true,
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке категорий: {ex.Message}"
                });
            }
        }

        private static string GetCategoryIcon(int categoryId) => categoryId switch
        {
            1 => "💄",  
            2 => "✨",  
            3 => "🌺",  
            4 => "💆‍♀️", 
            5 => "🛁",  
            6 => "🌟",  
            _ => "📦"   
        };


        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id_Category == id)
                    .Select(c => new CategoryDTO
                    {
                        IdCategory = c.Id_Category,
                        NameCa = c.NameCa,
                        DescriptionCa = c.DescriptionCa,
                        ProductCount = _context.Products.Count(p => p.CategoryID == c.Id_Category && p.IsAvailable)
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Категория не найдена"
                    });

                category.Icon = GetCategoryIcon(category.IdCategory);

                return Ok(new ApiResponse<CategoryDTO>
                {
                    Success = true,
                    Data = category
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке категории: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id_Category }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id_Category) return BadRequest();

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
