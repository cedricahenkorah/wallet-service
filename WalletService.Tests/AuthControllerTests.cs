using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WalletService.API.Controllers;
using WalletService.API.DTOs;
using WalletService.API.Services;
using Xunit;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsOk_WhenUserIsRegistered()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        var expectedResult = new UserResponseDto { Id = "1", PhoneNumber = userDto.PhoneNumber };
        _mockAuthService.Setup(s => s.RegisterAsync(userDto)).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RegisterAsync(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        _mockAuthService.Setup(s => s.RegisterAsync(userDto)).ReturnsAsync((UserResponseDto)null);

        // Act
        var result = await _controller.RegisterAsync(userDto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        _mockAuthService
            .Setup(s => s.RegisterAsync(userDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.RegisterAsync(userDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ReturnsOk_WhenUserLogsInSuccessfully()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        var expectedToken = "jwt_token";
        _mockAuthService.Setup(s => s.LoginAsync(userDto)).ReturnsAsync(expectedToken);

        // Act
        var result = await _controller.LoginAsync(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedToken, okResult.Value);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenLoginFails()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "wrongpassword" };
        _mockAuthService.Setup(s => s.LoginAsync(userDto)).ReturnsAsync((string)null);

        // Act
        var result = await _controller.LoginAsync(userDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        _mockAuthService
            .Setup(s => s.LoginAsync(userDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.LoginAsync(userDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
