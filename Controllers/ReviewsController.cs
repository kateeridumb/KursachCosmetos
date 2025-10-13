using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticShopAPI.Models;
using CosmeticShopAPI.DTOs;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public ReviewsController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDTO>>>> GetReviews()
        {
            try
            {
                var reviews = await _context.Reviews
                    .ToListAsync();

                var dtos = reviews.Select(r => new ReviewDTO
                {
                    IdReview = r.Id_Review,
                    ProductId = r.ProductID,
                    UserId = r.UserID,
                    Rating = r.Rating,
                    CommentRe = r.CommentRe,
                    CreatedAt = r.CreatedAt,
                    UserName = GetUserName(r.UserID)
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<ReviewDTO>>
                {
                    Success = true,
                    Data = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке отзывов: {ex.Message}"
                });
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDTO>>>> GetReviewsByProduct(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductID == productId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var dtos = reviews.Select(r => new ReviewDTO
                {
                    IdReview = r.Id_Review,
                    ProductId = r.ProductID,
                    UserId = r.UserID,
                    Rating = r.Rating,
                    CommentRe = r.CommentRe,
                    CreatedAt = r.CreatedAt,
                    UserName = GetUserName(r.UserID)
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<ReviewDTO>>
                {
                    Success = true,
                    Data = dtos,
                    TotalCount = dtos.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке отзывов: {ex.Message}"
                });
            }
        }

        [HttpGet("user/{userId}/product/{productId}")]
        public async Task<ActionResult<ApiResponse<ReviewDTO>>> GetUserReviewForProduct(int userId, int productId)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserID == userId && r.ProductID == productId);

                if (review == null)
                {
                    return Ok(new ApiResponse<ReviewDTO>
                    {
                        Success = true,
                        Data = null,
                        Message = "Отзыв не найден"
                    });
                }

                var dto = new ReviewDTO
                {
                    IdReview = review.Id_Review,
                    ProductId = review.ProductID,
                    UserId = review.UserID,
                    Rating = review.Rating,
                    CommentRe = review.CommentRe,
                    CreatedAt = review.CreatedAt,
                    UserName = GetUserName(review.UserID)
                };

                return Ok(new ApiResponse<ReviewDTO>
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
                    Message = $"Ошибка при проверке отзыва: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewDTO>>> GetReview(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.Id_Review == id);

                if (review == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Отзыв не найден"
                    });

                var dto = new ReviewDTO
                {
                    IdReview = review.Id_Review,
                    ProductId = review.ProductID,
                    UserId = review.UserID,
                    Rating = review.Rating,
                    CommentRe = review.CommentRe,
                    CreatedAt = review.CreatedAt,
                    UserName = GetUserName(review.UserID)
                };

                return Ok(new ApiResponse<ReviewDTO>
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
                    Message = $"Ошибка при загрузке отзыва: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReviewDTO>>> PostReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                // Проверяем, не оставлял ли пользователь уже отзыв на этот товар
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserID == request.UserId && r.ProductID == request.ProductId);

                if (existingReview != null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Вы уже оставляли отзыв на этот товар"
                    });
                }

                // Создаем DTO из запроса
                var dto = new ReviewDTO
                {
                    ProductId = request.ProductId,
                    UserId = request.UserId,
                    Rating = request.Rating,
                    CommentRe = request.CommentRe,
                    CreatedAt = DateTime.UtcNow
                };

                var sql = @"
            INSERT INTO Reviews (ProductID, UserID, Rating, CommentRe, CreatedAt)
            VALUES ({0}, {1}, {2}, {3}, {4})";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    dto.ProductId,
                    dto.UserId,
                    dto.Rating,
                    dto.CommentRe,
                    dto.CreatedAt);

                var newId = await _context.Reviews.MaxAsync(r => r.Id_Review);
                dto.IdReview = newId;
                dto.UserName = GetUserName(dto.UserId);

                return CreatedAtAction(nameof(GetReview), new { id = dto.IdReview },
                    new ApiResponse<ReviewDTO>
                    {
                        Success = true,
                        Data = dto,
                        Message = "Отзыв успешно добавлен"
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при создании отзыва: {ex.Message}"
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, ReviewUpdateDTO dto)
        {
            try
            {
                var sql = @"
                    UPDATE Reviews
                    SET Rating = {0}, CommentRe = {1}
                    WHERE Id_Review = {2}";

                var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                    dto.Rating,
                    dto.CommentRe,
                    id);

                if (rows == 0)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Отзыв не найден"
                    });

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Отзыв успешно обновлен"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при обновлении отзыва: {ex.Message}"
                });
            }
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        public async Task<IActionResult> DeleteUserReview(int userId, int productId)
        {
            try
            {
                Console.WriteLine($"=== API: УДАЛЕНИЕ ОТЗЫВА ===");
                Console.WriteLine($"User: {userId}, Product: {productId}");

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserID == userId && r.ProductID == productId);

                if (review == null)
                {
                    Console.WriteLine("❌ Отзыв не найден");
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Отзыв не найден"
                    });
                }

                Console.WriteLine($"✅ Найден отзыв ID: {review.Id_Review}");

                var sql = "DELETE FROM Reviews WHERE UserID = {0} AND ProductID = {1}";
                Console.WriteLine($"🗑️ Выполняем SQL: {sql}");
                Console.WriteLine($"🗑️ Параметры: UserID={userId}, ProductID={productId}");

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, userId, productId);

                Console.WriteLine($"✅ Удалено строк: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Отзыв успешно удален"
                    });
                }
                else
                {
                    Console.WriteLine("❌ Не удалось удалить отзыв (rowsAffected = 0)");
                    return StatusCode(500, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Не удалось удалить отзыв"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION: {ex}");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при удалении отзыва: {ex.Message}"
                });
            }
        }

        public class DeleteReviewRequest
        {
            public int UserId { get; set; }
            public int ProductId { get; set; }
        }

        private string GetUserName(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id_User == userId);
                return user != null ? $"{user.FirstName} {user.LastName}" : "Покупатель";
            }
            catch
            {
                return "Покупатель";
            }
        }
    }
}