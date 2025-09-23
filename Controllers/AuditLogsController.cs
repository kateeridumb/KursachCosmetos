using CosmeticShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AuditLogsController : ControllerBase
{
    private readonly CosmeticsShopDbContext _context;

    public AuditLogsController(CosmeticsShopDbContext context)
    {
        _context = context;
    }

    // GET: api/AuditLogs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
    {
        return await _context.AuditLogs.Include(a => a.User).ToListAsync();
    }

    // GET: api/AuditLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
    {
        var auditLog = await _context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(m => m.IdLog == id);

        if (auditLog == null)
            return NotFound();

        return auditLog;
    }

    // POST: api/AuditLogs
    [HttpPost]
    public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.IdLog }, auditLog);
    }

    // PUT: api/AuditLogs/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAuditLog(int id, AuditLog auditLog)
    {
        if (id != auditLog.IdLog)
            return BadRequest();

        _context.Entry(auditLog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.AuditLogs.Any(e => e.IdLog == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/AuditLogs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuditLog(int id)
    {
        var auditLog = await _context.AuditLogs.FindAsync(id);
        if (auditLog == null)
            return NotFound();

        _context.AuditLogs.Remove(auditLog);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
