using WalletService.API.DTOs;

namespace WalletService.API.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<UserResponseDto>> RegisterAsync(UserDto userDto);
        Task<ApiResponse<string>> LoginAsync(UserDto userDto);
    }
}
