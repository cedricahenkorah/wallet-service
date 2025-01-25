using MongoDB.Driver;
using WalletService.API.Configurations;
using WalletService.API.Models;

namespace WalletService.API.Repositories
{
    public class WalletRepository(MongoDbContext mongoDbContext) : IWalletRepository
    {
        private readonly IMongoCollection<Wallet> _wallets = mongoDbContext.Wallets;

        public async Task<Wallet> AddWalletAsync(Wallet wallet)
        {
            await _wallets.InsertOneAsync(wallet);
            return wallet;
        }

        public async Task<Wallet> GetWalletAsync(string id)
        {
            return await _wallets.Find(wallet => wallet.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Wallet>> GetUserWalletsAsync(
            string phoneNumber,
            int pageNumber,
            int pageSize
        )
        {
            return await _wallets
                .Find(wallet => wallet.Owner == phoneNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        async Task<List<Wallet>> IWalletRepository.GetWalletsAsync(int pageNumber, int pageSize)
        {
            return await _wallets
                .Find(wallet => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<bool> RemoveWalletAsync(string id)
        {
            var result = await _wallets.DeleteOneAsync(wallet => wallet.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> WalletExistsAsync(string accountNumber)
        {
            return await _wallets.Find(wallet => wallet.AccountNumber == accountNumber).AnyAsync();
        }

        public async Task<int> GetWalletCountForUserAsync(string owner)
        {
            return (int)await _wallets.Find(wallet => wallet.Owner == owner).CountDocumentsAsync();
        }

        public async Task<int> GetTotalWalletCountAsync()
        {
            return (int)await _wallets.CountDocumentsAsync(_ => true);
        }
    }
}
