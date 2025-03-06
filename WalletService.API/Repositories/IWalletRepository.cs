using WalletService.API.Models;

namespace WalletService.API.Repositories
{
    public interface IWalletRepository
    {
        Task<Wallet> AddWalletAsync(Wallet wallet);
        Task<bool> RemoveWalletAsync(string id);
        Task<Wallet> GetWalletAsync(string id);
        Task<List<Wallet>> GetWalletsAsync(int pageNumber, int pageSize);
        Task<List<Wallet>> GetUserWalletsAsync(string phoneNumber, int pageNumber, int pageSize);
        Task<bool> WalletExistsAsync(string accountNumber);
        Task<bool> WalletExistsName(string name);
        Task<int> GetWalletCountForUserAsync(string owner);
        Task<int> GetTotalWalletCountAsync();
    }
}
