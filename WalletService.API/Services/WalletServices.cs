using WalletService.API.DTOs;
using WalletService.API.Enums;
using WalletService.API.Models;
using WalletService.API.Repositories;

namespace WalletService.API.Services
{
    public class WalletServices(IWalletRepository walletRepository, ILogger<WalletServices> logger)
        : IWalletServices
    {
        private readonly IWalletRepository _walletRepository = walletRepository;
        private readonly ILogger<WalletServices> _logger = logger;

        public async Task<Wallet> AddWalletAsync(CreateWalletDto createWalletDto)
        {
            ArgumentNullException.ThrowIfNull(createWalletDto);

            // check enum values
            if (!Enum.IsDefined(typeof(WalletType), createWalletDto.Type))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid wallet type: {WalletType}",
                    createWalletDto.Type
                );
                throw new Exception("Invalid wallet type.");
            }

            if (!Enum.IsDefined(typeof(AccountScheme), createWalletDto.AccountScheme))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid account scheme: {AccountScheme}",
                    createWalletDto.AccountScheme
                );
                throw new Exception("Invalid account scheme.");
            }

            // if wallet type is card, account scheme should be visa or mastercard
            if (
                createWalletDto.Type == WalletType.Card
                && createWalletDto.AccountScheme != AccountScheme.Visa
                && createWalletDto.AccountScheme != AccountScheme.Mastercard
            )
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid account scheme for card wallet: {AccountScheme}",
                    createWalletDto.AccountScheme
                );
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
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid account scheme for momo wallet: {AccountScheme}",
                    createWalletDto.AccountScheme
                );
                throw new Exception("Invalid account scheme for momo wallet.");
            }

            // prevent duplicate wallet additions (check if it already exists)
            if (await _walletRepository.WalletExistsAsync(createWalletDto.AccountNumber))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Wallet with the same account number already exists: {AccountNumber}",
                    createWalletDto.AccountNumber
                );
                throw new Exception("Wallet with the same account number already exists.");
            }

            // a single user should not have more than 5 wallets
            if (await _walletRepository.GetWalletCountForUserAsync(createWalletDto.Owner) >= 5)
            {
                _logger.LogWarning(
                    "[AddWalletAsync] A user cannot have more than 5 wallets: {Owner}",
                    createWalletDto.Owner
                );
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

            return await _walletRepository.AddWalletAsync(wallet);
        }

        public async Task<List<Wallet>> GetWalletsAsync(int pageNumber, int pageSize)
        {
            return await _walletRepository.GetWalletsAsync(pageNumber, pageSize);
        }

        public async Task<List<Wallet>> GetUserWalletsAsync(
            string phoneNumber,
            int pageNumber,
            int pageSize
        )
        {
            // check if phone number is valid
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("[GetUserWalletsAsync] No phone number provided.");
                throw new Exception("No phone number provided.");
            }

            // check if user exists
            if (!await _walletRepository.WalletExistsAsync(phoneNumber))
            {
                _logger.LogWarning(
                    "[GetUserWalletsAsync] User does not exist: {PhoneNumber}",
                    phoneNumber
                );
                throw new Exception("User does not exist.");
            }

            return await _walletRepository.GetUserWalletsAsync(phoneNumber, pageNumber, pageSize);
        }

        public Task<Wallet> GetWalletAsync(string id)
        {
            // check if id is exists and is valid
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("[GetWalletAsync] No wallet id provided.");
                throw new Exception("No wallet id provided.");
            }

            // check if valid mongo id
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("[GetWalletAsync] Invalid wallet id.");
                throw new Exception("Invalid wallet id.");
            }

            return _walletRepository.GetWalletAsync(id);
        }

        public async Task<bool> RemoveWalletAsync(string id)
        {
            // check if id is exists and is valid
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("[RemoveWalletAsync] No wallet id provided.");
                throw new Exception("No wallet id provided.");
            }

            // check if valid mongo id
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("[RemoveWalletAsync] Invalid wallet id.");
                throw new Exception("Invalid wallet id.");
            }

            return await _walletRepository.RemoveWalletAsync(id);
            throw new NotImplementedException();
        }

        public async Task<int> GetTotalWalletCountAsync()
        {
            return await _walletRepository.GetTotalWalletCountAsync();
        }
    }
}
