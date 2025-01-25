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
        IWalletRepository walletRepository
    ) : ControllerBase
    {
        private readonly IWalletServices _walletServices = walletServices;
        private readonly IWalletRepository _walletRepository = walletRepository;

        private string GetUserPhoneNumber()
        {
            return User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> AddWalletAsync([FromBody] CreateWalletDto createWalletDto)
        {
            try
            {
                // check if the authenticated user is the owner of the wallet
                if (createWalletDto.Owner != GetUserPhoneNumber())
                {
                    return Unauthorized();
                }

                var wallet = await _walletServices.AddWalletAsync(createWalletDto);

                if (wallet == null)
                {
                    return BadRequest();
                }

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveWalletAsync(string id)
        {
            try
            {
                // check if the authenticated user is the owner of the wallet
                var wallet = await _walletServices.GetWalletAsync(id);

                if (wallet == null || wallet.Owner != GetUserPhoneNumber())
                {
                    return Unauthorized();
                }

                var result = await _walletServices.RemoveWalletAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletAsync(string id)
        {
            try
            {
                var wallet = await _walletServices.GetWalletAsync(id);

                if (wallet == null)
                {
                    return NotFound();
                }

                return Ok(wallet);
            }
            catch (Exception ex)
            {
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
            try
            {
                var wallets = await _walletServices.GetWalletsAsync(pageNumber, pageSize);

                if (wallets == null || wallets.Count == 0)
                {
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

                return Ok(response);
            }
            catch (Exception ex)
            {
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
                var wallets = await _walletServices.GetUserWalletsAsync(
                    phoneNumber,
                    pageNumber,
                    pageSize
                );

                if (wallets == null || wallets.Count == 0)
                {
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
    }
}
