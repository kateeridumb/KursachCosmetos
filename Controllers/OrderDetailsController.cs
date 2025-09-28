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
    public class OrderDetailsController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public OrderDetailsController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDTO>>> GetOrderDetails()
        {
            var orderDetails = await _context.OrderDetails.ToListAsync();

            var dtos = orderDetails.Select(od => new OrderDetailDTO
            {
                IdOrderDetail = od.Id_OrderDetail,
                OrderId = od.OrderID,
                ProductId = od.ProductID,
                Quantity = od.Quantity,
                Price = od.Price
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDTO>> GetOrderDetail(int id)
        {
            var od = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(od => od.Id_OrderDetail == id);

            if (od == null)
                return NotFound();

            var dto = new OrderDetailDTO
            {
                IdOrderDetail = od.Id_OrderDetail,
                OrderId = od.OrderID,
                ProductId = od.ProductID,
                Quantity = od.Quantity,
                Price = od.Price
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetailDTO>> PostOrderDetail(OrderDetailDTO dto)
        {
            var insertSql = @"
        INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price)
        VALUES ({0}, {1}, {2}, {3})";
            await _context.Database.ExecuteSqlRawAsync(insertSql,
                dto.OrderId, dto.ProductId, dto.Quantity, dto.Price);

            var newId = await _context.OrderDetails
                .OrderByDescending(od => od.Id_OrderDetail)
                .Select(od => od.Id_OrderDetail)
                .FirstAsync();

            dto.IdOrderDetail = newId;

            return CreatedAtAction(nameof(GetOrderDetail), new { id = dto.IdOrderDetail }, dto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderDetail(int id, OrderDetailDTO dto)
        {

            var sql = @"
             UPDATE OrderDetails
             SET OrderID = {0}, ProductID = {1}, Quantity = {2}, Price = {3}
             WHERE Id_OrderDetail = {4}";

            var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                dto.OrderId,
                dto.ProductId,
                dto.Quantity,
                dto.Price,
                id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var sql = "DELETE FROM OrderDetails WHERE ID_OrderDetail = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

    }
}
