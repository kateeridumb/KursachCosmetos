using CosmeticShopAPI.DTOs;
using CosmeticShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDTO>>>> GetProducts(
            [FromQuery] int? categoryId = null,
            [FromQuery] string search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (categoryId.HasValue && categoryId > 0)
                {
                    query = query.Where(p => p.CategoryID == categoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p =>
                        p.NamePr.Contains(search) ||
                        p.BrandPr.Contains(search) ||
                        p.DescriptionPr.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var products = await query
                    .OrderBy(p => p.NamePr)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var productDtos = products.Select(p => new ProductDTO
                {
                    IdProduct = p.Id_Product,
                    CategoryId = p.CategoryID,
                    NamePr = p.NamePr,
                    DescriptionPr = p.DescriptionPr,
                    BrandPr = p.BrandPr,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsAvailable = p.IsAvailable,
                    CategoryName = GetCategoryName(p.CategoryID),
                    CategoryIcon = GetCategoryIcon(p.CategoryID)
                }).ToList();

                var response = new ApiResponse<IEnumerable<ProductDTO>>
                {
                    Success = true,
                    Data = productDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке продуктов: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDTO>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id_Product == id);

                if (product == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Продукт не найден"
                    });
                }

                var dto = new ProductDTO
                {
                    IdProduct = product.Id_Product,
                    CategoryId = product.CategoryID,
                    NamePr = product.NamePr,
                    DescriptionPr = product.DescriptionPr,
                    BrandPr = product.BrandPr,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    IsAvailable = product.IsAvailable,
                    CategoryName = GetCategoryName(product.CategoryID),
                    CategoryIcon = GetCategoryIcon(product.CategoryID)
                };

                return Ok(new ApiResponse<ProductDTO>
                {
                    Success = true,
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке продукта: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при создании продукта: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            try
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
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Продукт не найден"
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при обновлении продукта: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var sql = "DELETE FROM Products WHERE Id_Product = {0}";
                var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

                if (rows == 0)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Продукт не найден"
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при удалении продукта: {ex.Message}"
                });
            }
        }

        private static string GetCategoryName(int categoryId) =>
           categoryId switch
           {
               1 => "Декоративная косметика",
               2 => "Уход за кожей",
               3 => "Парфюмерия",
               4 => "Уход за волосами",
               5 => "Уход за телом",
               6 => "Люкс косметика",
               _ => "Все товары"
           };

        private static string GetCategoryIcon(int categoryId) =>
            categoryId switch
            {
                1 => "💄",
                2 => "✨",
                3 => "🌺",
                4 => "💆‍♀️",
                5 => "🛁",
                6 => "🌟",
                _ => "📦"
            };
    }
}