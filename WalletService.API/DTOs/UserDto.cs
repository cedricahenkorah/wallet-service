using System.ComponentModel.DataAnnotations;

namespace WalletService.API.DTOs
{
    public class UserDto
    {
        public required string PhoneNumber { get; set; }

        [MinLength(6)]
        public required string Password { get; set; }
    }
}
