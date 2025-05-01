using MassTransit;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMassTransit(configurator =>
{
    //burda consumer yapilandirmasini sisteme bildirmemiz gerekiyor; asagidaki sekilde yapilir. ardindan bu consumer'in rabbitmq'da hang' kuyruktan islemi yapmasi gerektigini bildirmemiz gerekmektedir.
    configurator.AddConsumer<OrderCreatedEventConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);

        //mikroservisler arasinda servisler arasindaki kuyruk bilgileri, endpoint bilgileri, servisler arasindaki contract'i saglayacak tum bilgiler merkezi bir yerde tutulmali ve oradan cagrilmali.
        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
