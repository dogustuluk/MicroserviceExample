using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    readonly AppDbContext _context;

    public PaymentFailedEventConsumer(AppDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        Order.API.Models.Entities.Order order = await _context.Orders.FirstOrDefaultAsync(a => a.OrderId == context.Message.OrderId);
        order.OrderStatus = Models.Enums.OrderStatus.Failed;
        _context.SaveChangesAsync();
    }
}
