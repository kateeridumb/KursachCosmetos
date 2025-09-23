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
    public class PromoCodesController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public PromoCodesController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        // GET: api/PromoCodes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromoCode>>> GetPromoCodes()
        {
            return await _context.PromoCodes.ToListAsync();
        }

        // GET: api/PromoCodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PromoCode>> GetPromoCode(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);

            if (promoCode == null)
            {
                return NotFound();
            }

            return promoCode;
        }

        // POST: api/PromoCodes
        [HttpPost]
        public async Task<ActionResult<PromoCode>> PostPromoCode(PromoCode promoCode)
        {
            _context.PromoCodes.Add(promoCode);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPromoCode), new { id = promoCode.IdPromo }, promoCode);
        }

        // PUT: api/PromoCodes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromoCode(int id, PromoCode promoCode)
        {
            if (id != promoCode.IdPromo)
            {
                return BadRequest();
            }

            _context.Entry(promoCode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromoCodeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // стандартный ответ для PUT
        }

        // DELETE: api/PromoCodes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromoCode(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode == null)
            {
                return NotFound();
            }

            _context.PromoCodes.Remove(promoCode);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PromoCodeExists(int id)
        {
            return _context.PromoCodes.Any(e => e.IdPromo == id);
        }
    }
}
