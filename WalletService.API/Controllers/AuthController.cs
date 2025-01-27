using Microsoft.AspNetCore.Mvc;
using WalletService.API.DTOs;
using WalletService.API.Services;

namespace WalletService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger)
        : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserDto userDto)
        {
            _logger.LogInformation(
                "[RegisterAsync] Attempting to register user: {PhoneNumber}",
                userDto.PhoneNumber
            );

            try
            {
                var result = await _authService.RegisterAsync(userDto);

                return StatusCode(int.Parse(result.Code), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[RegisterAsync] An error occurred while registering user: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserDto userDto)
        {
            _logger.LogInformation(
                "[LoginAsync] Attempting to login user: {PhoneNumber}",
                userDto.PhoneNumber
            );
            try
            {
                var response = await _authService.LoginAsync(userDto);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[LoginAsync] An error occurred while logging in user: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
    }
}
