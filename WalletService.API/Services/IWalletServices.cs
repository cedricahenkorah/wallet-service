using WalletService.API.DTOs;
using WalletService.API.Models;

namespace WalletService.API.Services
{
    public interface IWalletServices
    {
        Task<ApiResponse<Wallet>> AddWalletAsync(CreateWalletDto createWalletDto);
        Task<ApiResponse<bool>> RemoveWalletAsync(string id);
        Task<ApiResponse<Wallet>> GetWalletAsync(string id);
        Task<ApiResponse<PaginatedResponse<Wallet>>> GetUserWalletsAsync(
            int pageNumber,
            int pageSizer
        );
        Task<ApiResponse<PaginatedResponse<Wallet>>> GetWalletsAsync(int pageNumber, int pageSize);
        Task<int> GetTotalWalletCountAsync();
        string GetUserPhoneNumber();
    }
}
