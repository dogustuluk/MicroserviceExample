using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    IMongoCollection<Stock.API.Models.Entities.Stock> _stockCollection;
    readonly ISendEndpointProvider _sendEndpointProvider;

    public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider)
    {
        _stockCollection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResult = new();

        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
            stockResult.Add((await _stockCollection.FindAsync(a => a.ProductId == orderItem.ProductId && a.Count >= orderItem.Count)).Any());
        }

        //stockResult icindeki tum degerler true ise siparis ile ilgili yanlis urun ve stok problemi yok ve siparis islemleri yapilabilir
        if (stockResult.TrueForAll(a => a.Equals(true)))
        {
            //gerekli siparis islemleri yapilir burda
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                Stock.API.Models.Entities.Stock stock = await (await _stockCollection.FindAsync(a => a.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                stock.Count -= orderItem.Count;
                await _stockCollection.FindOneAndReplaceAsync(a => a.ProductId == orderItem.ProductId, stock);
            }
            //guncelleme islemi sonrasi stok islemleri tamamlandigina dair payment islemleri yapilir.
            //payment islemleri
            StockReservedEvent stockReservedEvent = new StockReservedEvent()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                TotalPrice = context.Message.TotalPrice
            };

            //burada publish yerine send tipini kullaniyoruz cunku burada direkt kuyruk bazli bir modelleme yapiyoruz; sadece bu kuyrugu dinleyenler yakalayabilir bu eventi
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}")); //heventi hangi adrese gonderecegimizi belirleriz
            await sendEndpoint.Send(stockReservedEvent);

        }
        else
        {
            //sipariste urun id veya stok problemi varsa siparisi gecersiz kilmak icin islemler yapilir.
        }

        return Task.CompletedTask;
    }
}
