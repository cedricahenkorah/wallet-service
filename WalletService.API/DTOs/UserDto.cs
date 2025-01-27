using System.ComponentModel.DataAnnotations;

namespace WalletService.API.DTOs
{
    public class UserDto
    {
        [Required(ErrorMessage = "Phone number is required")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6)]
        public required string Password { get; set; }
    }
}
