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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
    {
        var logs = await _context.AuditLogs
            .FromSqlRaw("SELECT * FROM AuditLogs")
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
    {
        var log = await _context.AuditLogs
            .FromSqlRaw("SELECT * FROM AuditLogs WHERE Id_Log = {0}", id)
            .FirstOrDefaultAsync();

        if (log == null)
            return NotFound();

        return Ok(log);
    }

    [HttpPost]
    public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog auditLog)
    {
        var sql = @"INSERT INTO AuditLogs (UserID, UserName, TableName, ActionType, OldData, NewData, TimestampMl)
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6});";

        await _context.Database.ExecuteSqlRawAsync(
            sql,
            auditLog.UserID,
            auditLog.UserName,
            auditLog.TableName,
            auditLog.ActionType,
            auditLog.OldData,
            auditLog.NewData,
            auditLog.TimestampMl
        );

        var created = await _context.AuditLogs
            .FromSqlRaw("SELECT TOP 1 * FROM AuditLogs ORDER BY Id_Log DESC")
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetAuditLog), new { id = created!.Id_Log }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAuditLog(int id, AuditLog auditLog)
    {
        if (id != auditLog.Id_Log)
            return BadRequest();

        var sql = @"UPDATE AuditLogs
                    SET UserID = {0}, UserName = {1}, TableName = {2}, ActionType = {3},
                        OldData = {4}, NewData = {5}, TimestampMl = {6}
                    WHERE Id_Log = {7};";

        var affected = await _context.Database.ExecuteSqlRawAsync(
            sql,
            auditLog.UserID,
            auditLog.UserName,
            auditLog.TableName,
            auditLog.ActionType,
            auditLog.OldData,
            auditLog.NewData,
            auditLog.TimestampMl,
            id
        );

        if (affected == 0)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuditLog(int id)
    {
        var sql = "DELETE FROM AuditLogs WHERE Id_Log = {0}";
        var affected = await _context.Database.ExecuteSqlRawAsync(sql, id);

        if (affected == 0)
            return NotFound();

        return NoContent();
    }
}
