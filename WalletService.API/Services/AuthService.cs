using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WalletService.API.DTOs;
using WalletService.API.Models;
using WalletService.API.Repositories;

namespace WalletService.API.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor
    ) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<ApiResponse<string>> LoginAsync(UserDto userDto)
        {
            _logger.LogInformation(
                "[LoginAsync] Attempting to login user: {PhoneNumber}",
                userDto.PhoneNumber
            );

            ArgumentNullException.ThrowIfNull(userDto);

            try
            {
                var user = await _userRepository.GetUserByPhoneNumberAsync(userDto.PhoneNumber);

                if (user == null)
                {
                    _logger.LogWarning(
                        "[LoginAsync] User not found: {PhoneNumber}",
                        userDto.PhoneNumber
                    );
                    return new ApiResponse<string>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "User not found."
                    );
                }

                // check if password matches
                if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning(
                        "[LoginAsync] Invalid password for user: {PhoneNumber}",
                        userDto.PhoneNumber
                    );
                    return new ApiResponse<string>(
                        code: $"{(int)HttpStatusCode.BadRequest}",
                        message: "Invalid password."
                    );
                }

                // generate jwt token
                var token = GenerateJwtToken(user);

                if (token == null)
                {
                    _logger.LogWarning(
                        "[LoginAsync] Failed to generate jwt token for user: {PhoneNumber}",
                        userDto.PhoneNumber
                    );
                    return new ApiResponse<string>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "Failed to generate jwt token."
                    );
                }

                _logger.LogInformation(
                    "[LoginAsync] User logged in successfully: {PhoneNumber}",
                    userDto.PhoneNumber
                );

                return new ApiResponse<string>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "User logged in successfully.",
                    data: token
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[LoginAsync] An error occurred while logging in user: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return new ApiResponse<string>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while logging in user."
                );
            }
        }

        public async Task<ApiResponse<UserResponseDto>> RegisterAsync(UserDto userDto)
        {
            _logger.LogInformation(
                "[RegisterAsync] Attempting to register user: {PhoneNumber}",
                userDto.PhoneNumber
            );

            ArgumentNullException.ThrowIfNull(userDto);

            // check if user with the same phone number already exists
            if (await _userRepository.GetUserByPhoneNumberAsync(userDto.PhoneNumber) != null)
            {
                _logger.LogWarning(
                    "[RegisterAsync] User with the same phone number already exists: {PhoneNumber}",
                    userDto.PhoneNumber
                );

                return new ApiResponse<UserResponseDto>(
                    code: $"{(int)HttpStatusCode.BadRequest}",
                    message: "User with the same phone number already exists."
                );
            }

            // check if length of password is less than 6
            if (userDto.Password.Length < 6)
            {
                _logger.LogWarning(
                    "[RegisterAsync] Password length is less than 6 characters: {PhoneNumber}",
                    userDto.PhoneNumber
                );

                return new ApiResponse<UserResponseDto>(
                    code: $"{(int)HttpStatusCode.BadRequest}",
                    message: "Password length should be at least 6 characters."
                );
            }

            // hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // create new user
            var user = new User
            {
                PhoneNumber = userDto.PhoneNumber,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                await _userRepository.AddUserAsync(user);

                _logger.LogInformation(
                    "[RegisterAsync] User registered successfully: {PhoneNumber}",
                    userDto.PhoneNumber
                );

                return new ApiResponse<UserResponseDto>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "User registered successfully.",
                    data: new UserResponseDto { Id = user.Id, PhoneNumber = user.PhoneNumber }
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[RegisterAsync] An error occurred while registering user: {PhoneNumber}",
                    userDto.PhoneNumber
                );
                return new ApiResponse<UserResponseDto>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while registering user."
                );
            }
        }

        private string GenerateJwtToken(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            _logger.LogInformation(
                "[GenerateJwtToken] Generating jwt token for user: {PhoneNumber}",
                user.PhoneNumber
            );

            var jwtKey =
                _configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: [new Claim(ClaimTypes.Name, user.PhoneNumber)],
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            _logger.LogInformation(
                "[GenerateJwtToken] Jwt token generated successfully for user: {PhoneNumber}",
                user.PhoneNumber
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
