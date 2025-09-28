using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticShopAPI.Models;
using CosmeticShopAPI.DTOs;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromoCodesController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public PromoCodesController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromoCodeDTO>>> GetPromoCodes()
        {
            var sql = "SELECT Id_Promo, Code, DiscountPercent, MaxUsage, ExpiryDate, IsActive FROM PromoCodes";
            var promoCodes = await _context.PromoCodes
                .FromSqlRaw(sql)
                .Select(p => new PromoCodeDTO
                {
                    IdPromo = p.Id_Promo,
                    Code = p.Code,
                    DiscountPercent = p.DiscountPercent,
                    MaxUsage = p.MaxUsage,
                    ExpiryDate = p.ExpiryDate,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return promoCodes;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromoCodeDTO>> GetPromoCode(int id)
        {
            var sql = "SELECT Id_Promo, Code, DiscountPercent, MaxUsage, ExpiryDate, IsActive FROM PromoCodes WHERE Id_Promo = {0}";
            var promoCode = await _context.PromoCodes
                .FromSqlRaw(sql, id)
                .Select(p => new PromoCodeDTO
                {
                    IdPromo = p.Id_Promo,
                    Code = p.Code,
                    DiscountPercent = p.DiscountPercent,
                    MaxUsage = p.MaxUsage,
                    ExpiryDate = p.ExpiryDate,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();

            if (promoCode == null)
                return NotFound();

            return promoCode;
        }

        [HttpPost]
        public async Task<ActionResult<PromoCodeDTO>> PostPromoCode(PromoCodeDTO promoDto)
        {
            var sql = @"
                INSERT INTO PromoCodes (Code, DiscountPercent, MaxUsage, ExpiryDate, IsActive) 
                VALUES ({0}, {1}, {2}, {3}, {4});
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var newId = await _context.Database.ExecuteSqlRawAsync(sql,
                promoDto.Code,
                promoDto.DiscountPercent,
                promoDto.MaxUsage,
                promoDto.ExpiryDate,
                promoDto.IsActive
            );

            promoDto.IdPromo = newId;
            return CreatedAtAction(nameof(GetPromoCode), new { id = newId }, promoDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromoCode(int id, PromoCodeDTO promoDto)
        {
            var sql = @"
                UPDATE PromoCodes
                SET Code = {0}, DiscountPercent = {1}, MaxUsage = {2}, ExpiryDate = {3}, IsActive = {4}
                WHERE Id_Promo = {5}";

            var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                promoDto.Code,
                promoDto.DiscountPercent,
                promoDto.MaxUsage,
                promoDto.ExpiryDate,
                promoDto.IsActive,
                id
            );

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromoCode(int id)
        {
            var sqlUpdateOrders = "UPDATE Orders SET PromoID = NULL WHERE PromoID = {0}";
            await _context.Database.ExecuteSqlRawAsync(sqlUpdateOrders, id);

            var sqlDeletePromo = "DELETE FROM PromoCodes WHERE Id_Promo = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sqlDeletePromo, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
