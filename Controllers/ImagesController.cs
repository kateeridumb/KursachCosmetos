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
    public class ImagesController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public ImagesController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ImageDTO>>>> GetImages()
        {
            try
            {
                var images = await _context.Images.ToListAsync();

                var imageDtos = images.Select(i => new ImageDTO
                {
                    ID_Image = i.ID_Image,
                    ProductID = i.ProductID,
                    ImageURL = i.ImageURL,
                    DescriptionIMG = i.DescriptionIMG
                }).ToList();

                var response = new ApiResponse<IEnumerable<ImageDTO>>
                {
                    Success = true,
                    Data = imageDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке изображений: {ex.Message}"
                });
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ImageDTO>>>> GetImagesByProduct(int productId)
        {
            try
            {
                var images = await _context.Images
                    .Where(i => i.ProductID == productId)
                    .ToListAsync();

                var imageDtos = images.Select(i => new ImageDTO
                {
                    ID_Image = i.ID_Image,
                    ProductID = i.ProductID,
                    ImageURL = i.ImageURL,
                    DescriptionIMG = i.DescriptionIMG
                }).ToList();

                var response = new ApiResponse<IEnumerable<ImageDTO>>
                {
                    Success = true,
                    Data = imageDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Ошибка при загрузке изображений товара: {ex.Message}"
                });
            }
        }
    }
}