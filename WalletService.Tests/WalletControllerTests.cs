using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WalletService.API.Controllers;
using WalletService.API.DTOs;
using WalletService.API.Models;
using WalletService.API.Repositories;
using WalletService.API.Services;
using Xunit;

public class WalletControllerTests
{
    private readonly Mock<IWalletServices> _walletServicesMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ILogger<WalletController>> _loggerMock;
    private readonly WalletController _controller;

    public WalletControllerTests()
    {
        _walletServicesMock = new Mock<IWalletServices>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _loggerMock = new Mock<ILogger<WalletController>>();
        _controller = new WalletController(
            _walletServicesMock.Object,
            _walletRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    private void SetUserClaims(string phoneNumber)
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, phoneNumber) }, "mock")
        );
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user },
        };
    }

    [Fact]
    public async Task AddWalletAsync_ShouldReturnOk_WhenWalletIsCreated()
    {
        // Arrange
        var createWalletDto = new CreateWalletDto
        {
            Name = "Cedric Ahenkorah",
            Type = WalletService.API.Enums.WalletType.Momo,
            AccountNumber = "0234567891",
            AccountScheme = WalletService.API.Enums.AccountScheme.MTN,
            Owner = "+233123456789",
        };

        var wallet = new Wallet
        {
            Id = "1",
            Name = createWalletDto.Name,
            Type = createWalletDto.Type,
            AccountNumber =
                createWalletDto.Type == WalletService.API.Enums.WalletType.Card
                    ? createWalletDto.AccountNumber.Substring(0, 6)
                    : createWalletDto.AccountNumber,
            AccountScheme = createWalletDto.AccountScheme,
            Owner = createWalletDto.Owner,
            CreatedAt = DateTime.UtcNow,
        };

        _walletServicesMock.Setup(s => s.AddWalletAsync(createWalletDto)).ReturnsAsync(wallet);

        SetUserClaims(createWalletDto.Owner);

        // Act
        var result = await _controller.AddWalletAsync(createWalletDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedWallet = Assert.IsType<Wallet>(okResult.Value);
        Assert.Equal(wallet.Id, returnedWallet.Id);
    }

    [Fact]
    public async Task AddWalletAsync_ShouldReturnUnauthorized_WhenUserIsNotOwner()
    {
        // Arrange
        var createWalletDto = new CreateWalletDto { Owner = "+233987654321" };
        SetUserClaims("+233123456789");

        // Act
        var result = await _controller.AddWalletAsync(createWalletDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RemoveWalletAsync_ShouldReturnNoContent_WhenWalletIsRemoved()
    {
        // Arrange
        var wallet = new Wallet { Id = "1", Owner = "+233123456789" };
        SetUserClaims(wallet.Owner);

        _walletServicesMock.Setup(s => s.GetWalletAsync(wallet.Id)).ReturnsAsync(wallet);
        _walletServicesMock.Setup(s => s.RemoveWalletAsync(wallet.Id)).ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveWalletAsync(wallet.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveWalletAsync_ShouldReturnUnauthorized_WhenUserIsNotOwner()
    {
        // Arrange
        var wallet = new Wallet { Id = "1", Owner = "+233987654321" };
        SetUserClaims("+233123456789");

        _walletServicesMock.Setup(s => s.GetWalletAsync(wallet.Id)).ReturnsAsync(wallet);

        // Act
        var result = await _controller.RemoveWalletAsync(wallet.Id);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetWalletAsync_ShouldReturnNotFound_WhenWalletDoesNotExist()
    {
        // Arrange
        _walletServicesMock.Setup(s => s.GetWalletAsync("1")).ReturnsAsync((Wallet?)null!);

        // Act
        var result = await _controller.GetWalletAsync("1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetWalletAsync_ShouldReturnOk_WhenWalletExists()
    {
        // Arrange
        var wallet = new Wallet { Id = "1", Owner = "+233123456789" };
        _walletServicesMock.Setup(s => s.GetWalletAsync(wallet.Id)).ReturnsAsync(wallet);

        // Act
        var result = await _controller.GetWalletAsync(wallet.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedWallet = Assert.IsType<Wallet>(okResult.Value);
        Assert.Equal(wallet.Id, returnedWallet.Id);
    }
}
