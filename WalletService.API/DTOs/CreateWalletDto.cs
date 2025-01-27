using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WalletService.API.Enums;

namespace WalletService.API.DTOs
{
    public class CreateWalletDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [EnumDataType(typeof(WalletType), ErrorMessage = "Type must be either 'Momo' or 'Card'")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WalletType Type { get; set; }

        [Required(ErrorMessage = "Account Number is required")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Account Scheme is required")]
        [EnumDataType(typeof(AccountScheme), ErrorMessage = "Account Scheme is an invalid value.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountScheme AccountScheme { get; set; }

        [Required(ErrorMessage = "Owner is required")]
        public string Owner { get; set; }
    }
}
