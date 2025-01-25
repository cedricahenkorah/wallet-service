using WalletService.API.Models;

namespace WalletService.API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
        Task AddUserAsync(User user);
    }
}
