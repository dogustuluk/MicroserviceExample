using MassTransit;
using MongoDB.Driver;
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

// seed data
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!(await collection.Find(a => true).AnyAsync()))
{
    await collection.InsertOneAsync(new() { ProductId = "3731C522-5A3D-8B41-AF12-F25686740341", Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = "6F2EACBF-B1D7-8641-9DFF-4CF6823B0966", Count = 4000 });
    await collection.InsertOneAsync(new() { ProductId = "B1DC3471-75E9-934A-B41D-BC43DC9B56D2", Count = 2200 });
    await collection.InsertOneAsync(new() { ProductId = "179CB28B-91FE-BD42-A802-E11D8E2A68F3", Count = 3200 });
    await collection.InsertOneAsync(new() { ProductId = "9BA6711D-AE04-094F-BE1C-0A30718C1507", Count = 700 });
}
// /seed data



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
