using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WalletService.API.DTOs;
using WalletService.API.Services;

namespace WalletService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController(IWalletServices walletServices) : ControllerBase
    {
        private readonly IWalletServices _walletServices = walletServices;

        [HttpPost]
        public async Task<IActionResult> AddWalletAsync([FromBody] CreateWalletDto createWalletDto)
        {
            try
            {
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
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletsAsync()
        {
            try
            {
                var wallets = await _walletServices.GetWalletsAsync();
                return Ok(wallets);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
