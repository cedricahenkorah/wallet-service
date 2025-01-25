using WalletService.API.DTOs;
using WalletService.API.Models;

namespace WalletService.API.Services
{
    public interface IWalletServices
    {
        Task<Wallet> AddWalletAsync(CreateWalletDto createWalletDto);
        Task<bool> RemoveWalletAsync(string id);
        Task<Wallet> GetWalletAsync(string id);
        Task<List<Wallet>> GetWalletsAsync();
    }
}
