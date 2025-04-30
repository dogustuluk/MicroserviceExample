using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    readonly AppDbContext _context;
    readonly IPublishEndpoint _publishEndpoint;

    public OrderController(AppDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
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

        //event fırlat
        OrderCreatedEvent orderCreatedEvent = new()
        {
            BuyerId = order.BuyerId,
            OrderId = order.OrderId,
            OrderItems = order.OrderItems.Select(a => new Shared.Messages.OrderItemMessage
            {
                Count = a.Count,
                ProductId = a.ProductId
            }).ToList()
        };

        //publish et
        await _publishEndpoint.Publish(orderCreatedEvent);

        return Ok();
    }
}
