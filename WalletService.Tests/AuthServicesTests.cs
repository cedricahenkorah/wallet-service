using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WalletService.API.DTOs;
using WalletService.API.Models;
using WalletService.API.Repositories;
using WalletService.API.Services;

public class AuthServicesTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthService _authService;

    public AuthServicesTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // Mock JWT configuration
        _mockConfiguration
            .Setup(c => c["Jwt:Secret"])
            .Returns("SuperSecretKey12345678901234567890!@#");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockHttpContextAccessor.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        var user = new User
        {
            PhoneNumber = userDto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
        };

        _mockUserRepository
            .Setup(repo => repo.GetUserByPhoneNumberAsync(userDto.PhoneNumber))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(userDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal((int)HttpStatusCode.OK, int.Parse(result.Code));
        Assert.Equal("User logged in successfully.", result.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "wrongpassword" };
        var user = new User
        {
            PhoneNumber = userDto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                "password123"
            ) // Correct password
            ,
        };

        _mockUserRepository
            .Setup(repo => repo.GetUserByPhoneNumberAsync(userDto.PhoneNumber))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(userDto);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, int.Parse(result.Code));
        Assert.Equal("Invalid password.", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };

        _mockUserRepository
            .Setup(repo => repo.GetUserByPhoneNumberAsync(userDto.PhoneNumber))
            .ReturnsAsync((User)null);

        _mockUserRepository
            .Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(userDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal((int)HttpStatusCode.OK, int.Parse(result.Code));
        Assert.Equal("User registered successfully.", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_ExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "password123" };
        var existingUser = new User { PhoneNumber = userDto.PhoneNumber };

        _mockUserRepository
            .Setup(repo => repo.GetUserByPhoneNumberAsync(userDto.PhoneNumber))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(userDto);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, int.Parse(result.Code));
        Assert.Equal("User with the same phone number already exists.", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto { PhoneNumber = "1234567890", Password = "123" };

        // Act
        var result = await _authService.RegisterAsync(userDto);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, int.Parse(result.Code));
        Assert.Equal("Password length should be at least 6 characters.", result.Message);
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User { PhoneNumber = "1234567890" };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("1234567890", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
    }
}
