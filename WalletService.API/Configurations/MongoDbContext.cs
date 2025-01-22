using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WalletService.API.Models;

namespace WalletService.API.Configurations
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        }

        public IMongoCollection<Wallet> Wallets => _database.GetCollection<Wallet>("Wallets");

        public bool TestConnection()
        {
            try
            {
                // List collections as a test query
                _database.ListCollections();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
