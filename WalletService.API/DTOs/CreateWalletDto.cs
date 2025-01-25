using System.Text.Json.Serialization;
using WalletService.API.Enums;

namespace WalletService.API.DTOs
{
    public class CreateWalletDto
    {
        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WalletType Type { get; set; }

        public string AccountNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountScheme AccountScheme { get; set; }

        public string Owner { get; set; }
    }
}
