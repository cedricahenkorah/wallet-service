using WalletService.API.Models;

namespace WalletService.API.Repositories
{
    public interface IWalletRepository
    {
        Task<Wallet> AddWalletAsync(Wallet wallet);
        Task<bool> RemoveWalletAsync(string id);
        Task<Wallet> GetWalletAsync(string id);
        Task<List<Wallet>> GetWalletsAsync();
        Task<bool> WalletExistsAsync(string accountNumber);
        Task<int> GetWalletCountForUserAsync(string owner);
    }
}
