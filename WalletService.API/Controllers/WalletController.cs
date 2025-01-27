using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletService.API.DTOs;
using WalletService.API.Repositories;
using WalletService.API.Services;

namespace WalletService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController(IWalletServices walletServices, ILogger<WalletController> logger)
        : ControllerBase
    {
        private readonly IWalletServices _walletServices = walletServices;

        private readonly ILogger<WalletController> _logger = logger;

        private string GetUserPhoneNumber()
        {
            return User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> AddWalletAsync([FromBody] CreateWalletDto createWalletDto)
        {
            _logger.LogInformation(
                "[AddWalletAsync] Attempting to create wallet for user: {PhoneNumber}",
                createWalletDto.Owner
            );
            try
            {
                var response = await _walletServices.AddWalletAsync(createWalletDto);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[AddWalletAsync] An error occurred while creating wallet for user: {PhoneNumber}",
                    createWalletDto.Owner
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveWalletAsync(string id)
        {
            _logger.LogInformation(
                "[RemoveWalletAsync] Attempting to remove wallet: {WalletId}",
                id
            );

            try
            {
                var response = await _walletServices.RemoveWalletAsync(id);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[RemoveWalletAsync] An error occurred while removing wallet: {WalletId}",
                    id
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletAsync(string id)
        {
            _logger.LogInformation("[GetWalletAsync] Attempting to get wallet: {WalletId}", id);
            try
            {
                var response = await _walletServices.GetWalletAsync(id);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GetWalletAsync] An error occurred while getting wallet: {WalletId}",
                    id
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWalletsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            _logger.LogInformation("[GetWalletsAsync] Attempting to get wallets.");
            try
            {
                var response = await _walletServices.GetWalletsAsync(pageNumber, pageSize);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetWalletsAsync] An error occurred while getting wallets.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserWalletsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var response = await _walletServices.GetUserWalletsAsync(pageNumber, pageSize);

                return StatusCode(int.Parse(response.Code), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GetUserWalletsAsync] An error occurred while getting wallets for user: {PhoneNumber}",
                    GetUserPhoneNumber()
                );
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
    }
}
