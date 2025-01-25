using WalletService.API.DTOs;

namespace WalletService.API.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(UserDto userDto);
        Task<string> LoginAsync(UserDto userDto);
    }
}
