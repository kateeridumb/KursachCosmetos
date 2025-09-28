using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticShopAPI.Models;

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
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            await SetSessionUser();

            var sql = "INSERT INTO Categories (NameCa, DescriptionCa) VALUES ({0}, {1});";
            await _context.Database.ExecuteSqlRawAsync(sql, category.NameCa, category.DescriptionCa);

            var newId = await _context.Categories.MaxAsync(c => c.Id_Category);
            category.Id_Category = newId;

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id_Category }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id_Category) return BadRequest();

            await SetSessionUser();

            var sql = "UPDATE Categories SET NameCa = {0}, DescriptionCa = {1} WHERE ID_Category = {2};";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, category.NameCa, category.DescriptionCa, id);

            if (rows == 0) return NotFound();

            var updated = await _context.Categories.FindAsync(id);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await SetSessionUser();

            var sql = "DELETE FROM Categories WHERE Id_Category = {0};";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0) return NotFound();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id_Category == id);
        }

        private async Task SetSessionUser()
        {
            var userId = 1;
            var userName = "admin";

            var sql = "EXEC sp_set_session_context @key=N'UserID', @value={0}; " +
                      "EXEC sp_set_session_context @key=N'UserName', @value={1};";
            await _context.Database.ExecuteSqlRawAsync(sql, userId, userName);
        }
    }
}
