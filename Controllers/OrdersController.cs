using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopApi.Data;
using ShopApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext db, ILogger<OrdersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all orders.");

            var orders = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} orders.", orders.Count);

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            _logger.LogInformation("Fetching order with id {OrderId}", id);

            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order with id {OrderId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Order with id {OrderId} fetched successfully.", id);

            return Ok(order);
        }

        public record OrderItemDto(int ProductId, int Quantity);

        public class CreateOrderDto
        {
            public int UserId { get; set; }
            public OrderItemDto[] Items { get; set; } = new OrderItemDto[0];
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            _logger.LogInformation(
                "Creating order for user {UserId} with {ItemCount} items.",
                dto.UserId,
                dto.Items.Length);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("Failed to create order. User {UserId} not found.", dto.UserId);
                return BadRequest("User not found");
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var it in dto.Items)
            {
                if (!products.ContainsKey(it.ProductId))
                {
                    _logger.LogWarning(
                        "Failed to create order. Product {ProductId} not found.",
                        it.ProductId);
                    return BadRequest($"Product {it.ProductId} not found");
                }
            }

            var order = new Order { UserId = dto.UserId };

            foreach (var it in dto.Items)
            {
                var prod = products[it.ProductId];
                order.Items.Add(new OrderItem
                {
                    ProductId = prod.Id,
                    Quantity = it.Quantity,
                    UnitPrice = prod.Price
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} created successfully for user {UserId}.",
                order.Id,
                dto.UserId);

            await _db.Entry(order).Reference(o => o.User).LoadAsync();
            await _db.Entry(order).Collection(o => o.Items).Query().Include(i => i.Product).LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting order with id {OrderId}", id);

            var o = await _db.Orders.FindAsync(id);
            if (o == null)
            {
                _logger.LogWarning("Cannot delete. Order with id {OrderId} not found.", id);
                return NotFound();
            }

            _db.Orders.Remove(o);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} deleted successfully.", id);

            return NoContent();
        }
    }
}
