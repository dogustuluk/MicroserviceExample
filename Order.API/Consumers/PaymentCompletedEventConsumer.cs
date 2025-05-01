using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    readonly AppDbContext _context;

    public PaymentCompletedEventConsumer(AppDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        Order.API.Models.Entities.Order order = await _context.Orders.FirstOrDefaultAsync(a => a.OrderId == context.Message.OrderId);
        order.OrderStatus = Models.Enums.OrderStatus.Completed;

        await _context.SaveChangesAsync();

    }
}
