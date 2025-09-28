using CosmeticShop.Core.DTOs;
using CosmeticShopAPI.DTOs;
using CosmeticShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public ProductsController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            var dtos = products.Select(p => new ProductDTO
            {
                IdProduct = p.Id_Product,
                CategoryId = p.CategoryID,
                NamePr = p.NamePr,
                DescriptionPr = p.DescriptionPr,
                BrandPr = p.BrandPr,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsAvailable = p.IsAvailable
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id_Product == id);

            if (product == null)
                return NotFound();

            var dto = new ProductDTO
            {
                IdProduct = product.Id_Product,
                CategoryId = product.CategoryID,
                NamePr = product.NamePr,
                DescriptionPr = product.DescriptionPr,
                BrandPr = product.BrandPr,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var sql = @"
                INSERT INTO Products (CategoryID, NamePr, DescriptionPr, BrandPr, Price, StockQuantity, IsAvailable)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})";

            await _context.Database.ExecuteSqlRawAsync(sql,
                product.CategoryID,
                product.NamePr,
                product.DescriptionPr,
                product.BrandPr,
                product.Price,
                product.StockQuantity,
                product.IsAvailable);

            var newId = await _context.Products.MaxAsync(p => p.Id_Product);
            product.Id_Product = newId;

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id_Product }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            var sql = @"
                UPDATE Products
                SET CategoryID = {0}, NamePr = {1}, DescriptionPr = {2}, BrandPr = {3}, 
                    Price = {4}, StockQuantity = {5}, IsAvailable = {6}
                WHERE Id_Product = {7}";

            var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                product.CategoryID,
                product.NamePr,
                product.DescriptionPr,
                product.BrandPr,
                product.Price,
                product.StockQuantity,
                product.IsAvailable,
                id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var sql = "DELETE FROM Products WHERE Id_Product = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
