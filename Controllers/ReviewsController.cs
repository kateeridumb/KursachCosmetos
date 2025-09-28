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
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviews()
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
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id_Review == id);

            if (review == null)
                return NotFound();

            var dto = new ReviewDTO
            {
                IdReview = review.Id_Review,
                ProductId = review.ProductID,
                UserId = review.UserID,
                Rating = review.Rating,
                CommentRe = review.CommentRe,
                CreatedAt = review.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDTO>> PostReview(ReviewDTO dto)
        {
            dto.CreatedAt = DateTime.UtcNow;

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

            return CreatedAtAction(nameof(GetReview), new { id = dto.IdReview }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, ReviewUpdateDTO dto)
        {

            var sql = @"
                UPDATE Reviews
                SET ProductID = {0}, UserID = {1}, Rating = {2}, CommentRe = {3}
                WHERE Id_Review = {4}";

            var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                dto.ProductId,
                dto.UserId,
                dto.Rating,
                dto.CommentRe,
                id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var sql = "DELETE FROM Reviews WHERE Id_Review = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
