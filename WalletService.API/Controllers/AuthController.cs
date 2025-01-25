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

                if (result == null)
                {
                    _logger.LogWarning(
                        "[RegisterAsync] User registration failed: {PhoneNumber}",
                        userDto.PhoneNumber
                    );
                    return BadRequest();
                }

                _logger.LogInformation(
                    "[RegisterAsync] User registered successfully: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return Ok(result);
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
                var token = await _authService.LoginAsync(userDto);

                if (token == null)
                {
                    _logger.LogWarning(
                        "[LoginAsync] Unauthorized. User login failed: {PhoneNumber}",
                        userDto.PhoneNumber
                    );
                    return Unauthorized();
                }

                _logger.LogInformation(
                    "[LoginAsync] User logged in successfully: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return Ok(token);
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
