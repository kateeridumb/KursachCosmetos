using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticShopAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmeticShopAPI.DTOs;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public ImagesController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetImages()
        {
            var images = await _context.Images.ToListAsync();

            var imageDtos = images.Select(i => new ImageDTO
            {
                IdImage = i.Id_Image,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                DescriptionImg = i.DescriptionImg
            }).ToList();

            return Ok(imageDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImageDTO>> GetImage(int id)
        {
            var image = await _context.Images
                .Where(i => i.Id_Image == id)
                .FirstOrDefaultAsync();

            if (image == null)
                return NotFound();

            var dto = new ImageDTO
            {
                IdImage = image.Id_Image,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                DescriptionImg = image.DescriptionImg
            };

            return Ok(dto);
        }


        [HttpPost]
        public async Task<ActionResult<ImageDTO>> PostImage(ImageDTO dto)
        {
            var image = new Image
            {
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                DescriptionImg = dto.DescriptionImg
            };

            var sql = "INSERT INTO Images (ProductId, ImageUrl, DescriptionImg) VALUES ({0}, {1}, {2})";
            await _context.Database.ExecuteSqlRawAsync(sql, image.ProductId, image.ImageUrl, image.DescriptionImg);

            var newId = await _context.Images.MaxAsync(i => i.Id_Image);
            dto.IdImage = newId;

            return CreatedAtAction(nameof(GetImage), new { id = dto.IdImage }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutImage(int id, ImageDTO dto)
        {
            var image = new Image
            {
                Id_Image = id,
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                DescriptionImg = dto.DescriptionImg
            };

            var sql = "UPDATE Images SET ProductId = {0}, ImageUrl = {1}, DescriptionImg = {2} WHERE Id_Image = {3}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, image.ProductId, image.ImageUrl, image.DescriptionImg, image.Id_Image);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var sql = "DELETE FROM Images WHERE Id_Image = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
