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
    public class WalletController(
        IWalletServices walletServices,
        IWalletRepository walletRepository,
        ILogger<WalletController> logger
    ) : ControllerBase
    {
        private readonly IWalletServices _walletServices = walletServices;
        private readonly IWalletRepository _walletRepository = walletRepository;
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
                // check if the authenticated user is the owner of the wallet
                if (createWalletDto.Owner != GetUserPhoneNumber())
                {
                    _logger.LogWarning(
                        "[AddWalletAsync] Unauthorized attempt to create wallet for user: {PhoneNumber}",
                        createWalletDto.Owner
                    );
                    return Unauthorized();
                }

                var wallet = await _walletServices.AddWalletAsync(createWalletDto);

                if (wallet == null)
                {
                    _logger.LogWarning(
                        "[AddWalletAsync] Wallet creation failed for user: {PhoneNumber}",
                        createWalletDto.Owner
                    );
                    return BadRequest();
                }

                _logger.LogInformation(
                    "[AddWalletAsync] Wallet created successfully for user: {PhoneNumber}",
                    createWalletDto.Owner
                );
                return Ok(wallet);
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
                // check if the authenticated user is the owner of the wallet
                var wallet = await _walletServices.GetWalletAsync(id);

                if (wallet == null || wallet.Owner != GetUserPhoneNumber())
                {
                    _logger.LogWarning(
                        "[RemoveWalletAsync] Unauthorized attempt to remove wallet: {WalletId}",
                        id
                    );
                    return Unauthorized();
                }

                var result = await _walletServices.RemoveWalletAsync(id);

                if (!result)
                {
                    _logger.LogWarning("[RemoveWalletAsync] Wallet removal failed: {WalletId}", id);
                    return NotFound();
                }

                _logger.LogInformation(
                    "[RemoveWalletAsync] Wallet removed successfully: {WalletId}",
                    id
                );
                return NoContent();
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
                var wallet = await _walletServices.GetWalletAsync(id);

                if (wallet == null)
                {
                    _logger.LogWarning("[GetWalletAsync] Wallet not found: {WalletId}", id);
                    return NotFound();
                }

                _logger.LogInformation(
                    "[GetWalletAsync] Wallet retrieved successfully: {WalletId}",
                    id
                );
                return Ok(wallet);
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
                var wallets = await _walletServices.GetWalletsAsync(pageNumber, pageSize);

                if (wallets == null || wallets.Count == 0)
                {
                    _logger.LogWarning("[GetWalletsAsync] No wallets found.");
                    return NotFound("No wallets found.");
                }

                var totalCount = await _walletServices.GetTotalWalletCountAsync();

                var response = new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Wallets = wallets,
                };

                _logger.LogInformation("[GetWalletsAsync] Wallets retrieved successfully.");
                return Ok(response);
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
                var phoneNumber = GetUserPhoneNumber();

                _logger.LogInformation(
                    "[GetUserWalletsAsync] Attempting to get wallets for user: {PhoneNumber}",
                    phoneNumber
                );

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogWarning(
                        "[GetUserWalletsAsync] Unauthorized attempt to get wallets for user: {PhoneNumber}",
                        phoneNumber
                    );
                    return Unauthorized();
                }

                var wallets = await _walletServices.GetUserWalletsAsync(
                    phoneNumber,
                    pageNumber,
                    pageSize
                );

                if (wallets == null || wallets.Count == 0)
                {
                    _logger.LogWarning(
                        "[GetUserWalletsAsync] No wallets found for user: {PhoneNumber}",
                        phoneNumber
                    );
                    return NotFound("No wallets found for this user.");
                }

                var totalCount = await _walletRepository.GetWalletCountForUserAsync(phoneNumber);

                var response = new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Wallets = wallets,
                };

                _logger.LogInformation(
                    "[GetUserWalletsAsync] Wallets retrieved successfully for user: {PhoneNumber}",
                    phoneNumber
                );
                return Ok(response);
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
