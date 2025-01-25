using WalletService.API.DTOs;
using WalletService.API.Enums;
using WalletService.API.Models;
using WalletService.API.Repositories;

namespace WalletService.API.Services
{
    public class WalletServices(IWalletRepository walletRepository) : IWalletServices
    {
        private readonly IWalletRepository _walletRepository = walletRepository;

        public async Task<Wallet> AddWalletAsync(CreateWalletDto createWalletDto)
        {
            ArgumentNullException.ThrowIfNull(createWalletDto);

            // check enum values
            if (!Enum.IsDefined(typeof(WalletType), createWalletDto.Type))
            {
                throw new Exception("Invalid wallet type.");
            }

            if (!Enum.IsDefined(typeof(AccountScheme), createWalletDto.AccountScheme))
            {
                throw new Exception("Invalid account scheme.");
            }

            // if wallet type is card, account scheme should be visa or mastercard
            if (
                createWalletDto.Type == WalletType.Card
                && createWalletDto.AccountScheme != AccountScheme.Visa
                && createWalletDto.AccountScheme != AccountScheme.Mastercard
            )
            {
                throw new Exception("Invalid account scheme for card wallet.");
            }

            // if wallet type is momo, account scheme should be MTN, Vodafone or AirtelTigo
            if (
                createWalletDto.Type == WalletType.Momo
                && createWalletDto.AccountScheme != AccountScheme.MTN
                && createWalletDto.AccountScheme != AccountScheme.Vodafone
                && createWalletDto.AccountScheme != AccountScheme.AirtelTigo
            )
            {
                throw new Exception("Invalid account scheme for momo wallet.");
            }

            // prevent duplicate wallet additions (check if it already exists)
            if (await _walletRepository.WalletExistsAsync(createWalletDto.AccountNumber))
            {
                throw new Exception("Wallet with the same account number already exists.");
            }

            // a single user should not have more than 5 wallets
            if (await _walletRepository.GetWalletCountForUserAsync(createWalletDto.Owner) >= 5)
            {
                throw new Exception("A user cannot have more than 5 wallets.");
            }

            // if wallet is a visa or mastercard, only first 6 digits of card number should be stored
            var wallet = new Wallet
            {
                Name = createWalletDto.Name,
                Type = createWalletDto.Type,
                AccountNumber =
                    createWalletDto.Type == WalletType.Card
                        ? createWalletDto.AccountNumber.Substring(0, 6)
                        : createWalletDto.AccountNumber,
                AccountScheme = createWalletDto.AccountScheme,
                Owner = createWalletDto.Owner,
                CreatedAt = DateTime.UtcNow,
            };

            Console.WriteLine($"Type: {wallet.Type}, AccountScheme: {wallet.AccountScheme}");

            return await _walletRepository.AddWalletAsync(wallet);
        }

        public Task<Wallet> GetWalletAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Wallet>> GetWalletsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveWalletAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
