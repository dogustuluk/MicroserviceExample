using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDbService
{
    readonly IMongoDatabase _database;

    public MongoDbService(IConfiguration configuration)
    {
        MongoClient client = new MongoClient(configuration.GetConnectionString("MongoDB"));
        _database = client.GetDatabase("StockAPIDB");
    }
}
