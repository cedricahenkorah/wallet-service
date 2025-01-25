using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WalletService.API.DTOs;
using WalletService.API.Models;
using WalletService.API.Repositories;

namespace WalletService.API.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration configuration)
        : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> LoginAsync(UserDto userDto)
        {
            ArgumentNullException.ThrowIfNull(userDto);

            // check if user exists
            var user =
                await _userRepository.GetUserByPhoneNumberAsync(userDto.PhoneNumber)
                ?? throw new Exception("User not found.");

            // check if password matches
            if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid password.");
            }

            // generate jwt token
            return GenerateJwtToken(user);
        }

        public async Task<UserResponseDto> RegisterAsync(UserDto userDto)
        {
            ArgumentNullException.ThrowIfNull(userDto);

            // check if user with the same phone number already exists
            if (await _userRepository.GetUserByPhoneNumberAsync(userDto.PhoneNumber) != null)
            {
                throw new Exception("User with the same phone number already exists.");
            }

            // check if length of password is less than 6
            if (userDto.Password.Length < 6)
            {
                throw new Exception("Password length should be at least 6 characters.");
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

            await _userRepository.AddUserAsync(user);

            return new UserResponseDto { Id = user.Id, PhoneNumber = user.PhoneNumber };
        }

        private string GenerateJwtToken(User user)
        {
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
