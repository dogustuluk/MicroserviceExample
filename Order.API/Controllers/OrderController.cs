using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Enums;
using Order.API.ViewModels;

namespace Order.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderVM model)
    {
        Order.API.Models.Entities.Order order = new()
        {
            OrderId = Guid.NewGuid(),
            BuyerId = model.BuyerId,
            CreatedDate = DateTime.Now,
            OrderStatus = OrderStatus.Suspend
        };

        order.OrderItems = model.OrderItems.Select(a => new Models.Entities.OrderItem
        {
            Count = a.Count,
            Price = a.Price,
            ProductId = a.ProductId
        }).ToList();

        order.TotalPrice = model.OrderItems.Sum(a => (a.Price * a.Count));

        await _context.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
