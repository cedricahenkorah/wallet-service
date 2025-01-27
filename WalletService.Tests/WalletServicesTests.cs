using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WalletService.API.DTOs;
using WalletService.API.Enums;
using WalletService.API.Models;
using WalletService.API.Repositories;
using WalletService.API.Services;

public class WalletServicesTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ILogger<WalletServices>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly WalletServices _walletServices;

    public WalletServicesTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _loggerMock = new Mock<ILogger<WalletServices>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _walletServices = new WalletServices(
            _walletRepositoryMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    [Fact]
    public async Task AddWalletAsync_ShouldReturnUnauthorized_WhenOwnerMismatch()
    {
        // Arrange
        var createWalletDto = new CreateWalletDto
        {
            Owner = "1234567890",
            Type = WalletType.Momo,
            AccountScheme = AccountScheme.MTN,
            AccountNumber = "1234567890123",
        };

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "0987654321") };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        // Act
        var result = await _walletServices.AddWalletAsync(createWalletDto);

        // Assert
        Assert.Equal(((int)HttpStatusCode.Unauthorized).ToString(), result.Code);
        Assert.Equal("Unauthorized attempt to create wallet", result.Message);
    }

    [Fact]
    public async Task AddWalletAsync_ShouldReturnConflict_WhenWalletExists()
    {
        // Arrange
        var createWalletDto = new CreateWalletDto
        {
            Owner = "1234567890",
            Type = WalletType.Momo,
            AccountScheme = AccountScheme.MTN,
            AccountNumber = "1234567890123",
        };

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "1234567890") };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);
        _walletRepositoryMock
            .Setup(repo => repo.WalletExistsAsync(createWalletDto.AccountNumber))
            .ReturnsAsync(true);

        // Act
        var result = await _walletServices.AddWalletAsync(createWalletDto);

        // Assert
        Assert.Equal(((int)HttpStatusCode.Conflict).ToString(), result.Code);
        Assert.Equal("Wallet with the same account number already exists", result.Message);
    }

    [Fact]
    public async Task AddWalletAsync_ShouldReturnCreated_WhenWalletIsAddedSuccessfully()
    {
        // Arrange
        var createWalletDto = new CreateWalletDto
        {
            Owner = "1234567890",
            Type = WalletType.Momo,
            AccountScheme = AccountScheme.MTN,
            AccountNumber = "1234567890123",
        };

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "1234567890") };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);
        _walletRepositoryMock
            .Setup(repo => repo.WalletExistsAsync(createWalletDto.AccountNumber))
            .ReturnsAsync(false);
        _walletRepositoryMock
            .Setup(repo => repo.AddWalletAsync(It.IsAny<Wallet>()))
            .ReturnsAsync(new Wallet());

        // Act
        var result = await _walletServices.AddWalletAsync(createWalletDto);

        // Assert
        Assert.Equal(((int)HttpStatusCode.Created).ToString(), result.Code);
        Assert.Equal("Wallet added successfully", result.Message);
    }
}
