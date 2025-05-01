using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers;

public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
{
    readonly IPublishEndpoint _publishEndpoint;

    public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        if (true)
        {
            PaymentCompletedEvent paymentCompletedEvent = new()
            {
                OrderId = context.Message.OrderId
            };

            _publishEndpoint.Publish(paymentCompletedEvent);
        }
        else
        {

        }

        return Task.CompletedTask;
    }
}
