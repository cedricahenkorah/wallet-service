using System.Net;
using System.Security.Claims;
using WalletService.API.DTOs;
using WalletService.API.Enums;
using WalletService.API.Models;
using WalletService.API.Repositories;

namespace WalletService.API.Services
{
    public class WalletServices(
        IWalletRepository walletRepository,
        ILogger<WalletServices> logger,
        IHttpContextAccessor httpContextAccessor
    ) : IWalletServices
    {
        private readonly IWalletRepository _walletRepository = walletRepository;
        private readonly ILogger<WalletServices> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string GetUserPhoneNumber()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? string.Empty;
        }

        public async Task<ApiResponse<Wallet>> AddWalletAsync(CreateWalletDto createWalletDto)
        {
            // check if the authenticated user is the owner of the wallet
            if (createWalletDto.Owner != GetUserPhoneNumber())
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Unauthorized attempt to create wallet for user: {PhoneNumber}",
                    createWalletDto.Owner
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Unauthorized}",
                    message: "Unauthorized attempt to create wallet"
                );
            }

            ArgumentNullException.ThrowIfNull(createWalletDto);

            // check enum values
            if (!Enum.IsDefined(typeof(WalletType), createWalletDto.Type))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid wallet type: {WalletType}",
                    createWalletDto.Type
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid wallet type"
                );
            }

            if (!Enum.IsDefined(typeof(AccountScheme), createWalletDto.AccountScheme))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Invalid account scheme: {AccountScheme}",
                    createWalletDto.AccountScheme
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid account scheme"
                );
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

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid account scheme for card wallet"
                );
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

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid account scheme for momo wallet"
                );
            }

            // prevent duplicate wallet additions (check if it already exists)
            if (await _walletRepository.WalletExistsAsync(createWalletDto.AccountNumber))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Wallet with the same account number already exists: {AccountNumber}",
                    createWalletDto.AccountNumber
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Conflict}",
                    message: "Wallet with the same account number already exists"
                );
            }

            if (await _walletRepository.WalletExistsName(createWalletDto.Name))
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Wallet with the same name already exists: {Name}",
                    createWalletDto.Name
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Conflict}",
                    message: "Wallet with the same name already exists"
                );
            }

            // prevent duplicate wallet additions for card (check if first 6 digits already exists)
            if (
                createWalletDto.Type == WalletType.Card
                && await _walletRepository.WalletExistsAsync(
                    createWalletDto.AccountNumber.Substring(0, 6)
                )
            )
            {
                _logger.LogWarning(
                    "[AddWalletAsync] Wallet with the same card number already exists: {AccountNumber}",
                    createWalletDto.AccountNumber
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Conflict}",
                    message: "Wallet with the same card number already exists"
                );
            }

            // a single user should not have more than 5 wallets
            if (await _walletRepository.GetWalletCountForUserAsync(createWalletDto.Owner) >= 5)
            {
                _logger.LogWarning(
                    "[AddWalletAsync] A user cannot have more than 5 wallets: {Owner}",
                    createWalletDto.Owner
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Conflict}",
                    message: "A user cannot have more than 5 wallets"
                );
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

            try
            {
                var response = await _walletRepository.AddWalletAsync(wallet);

                if (response == null)
                {
                    _logger.LogWarning(
                        "[AddWalletAsync] An error occurred while adding wallet: {Wallet}",
                        wallet
                    );

                    return new ApiResponse<Wallet>(
                        code: $"{(int)HttpStatusCode.InternalServerError}",
                        message: "An error occurred while adding wallet"
                    );
                }

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.Created}",
                    message: "Wallet added successfully",
                    data: response
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[AddWalletAsync] An error occurred while adding wallet: {Wallet}",
                    wallet
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while adding wallet"
                );
            }
        }

        public async Task<ApiResponse<PaginatedResponse<Wallet>>> GetWalletsAsync(
            int pageNumber,
            int pageSize
        )
        {
            if (pageNumber < 1)
            {
                _logger.LogWarning(
                    "[GetWalletsAsync] Invalid page number: {PageNumber}",
                    pageNumber
                );
                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid page number"
                );
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("[GetWalletsAsync] Invalid page size: {PageSize}", pageSize);
                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid page size"
                );
            }

            try
            {
                var wallets = await _walletRepository.GetWalletsAsync(pageNumber, pageSize);

                if (wallets == null || wallets.Count == 0)
                {
                    _logger.LogWarning("[GetWalletsAsync] No wallets found.");
                    return new ApiResponse<PaginatedResponse<Wallet>>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "No wallets found"
                    );
                }

                var totalCount = await _walletRepository.GetTotalWalletCountAsync();

                var response = new PaginatedResponse<Wallet>
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = wallets,
                };

                _logger.LogInformation("[GetWalletsAsync] Wallets retrieved successfully.");

                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "Wallets retrieved successfully",
                    data: response
                );
            }
            catch (System.Exception)
            {
                _logger.LogError("[GetWalletsAsync] An error occurred while getting wallets.");

                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while getting wallets"
                );
            }
        }

        public async Task<ApiResponse<PaginatedResponse<Wallet>>> GetUserWalletsAsync(
            int pageNumber,
            int pageSize
        )
        {
            var phoneNumber = GetUserPhoneNumber();

            _logger.LogInformation(
                "[GetUserWalletsAsync] Attempting to get wallets for user: {PhoneNumber}",
                phoneNumber
            );

            // check if phone number is valid
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("[GetUserWalletsAsync] Unauthorized attempt to get wallets");
                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.Unauthorized}",
                    message: "Unauthorized attempt to get wallets"
                );
            }

            // check if page number is valid
            if (pageNumber < 1)
            {
                _logger.LogWarning(
                    "[GetUserWalletsAsync] Invalid page number: {PageNumber}",
                    pageNumber
                );
                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid page number"
                );
            }

            // check if page size is valid
            if (pageSize < 1)
            {
                _logger.LogWarning("[GetUserWalletsAsync] Invalid page size: {PageSize}", pageSize);
                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid page size"
                );
            }

            try
            {
                var wallets = await _walletRepository.GetUserWalletsAsync(
                    phoneNumber,
                    pageNumber,
                    pageSize
                );

                if (wallets == null || wallets.Count == 0)
                {
                    _logger.LogWarning(
                        "[GetUserWalletsAsync] No wallets found for user: {PhoneNumber}",
                        phoneNumber
                    );
                    return new ApiResponse<PaginatedResponse<Wallet>>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "No wallets found for this user"
                    );
                }

                var totalCount = await _walletRepository.GetWalletCountForUserAsync(phoneNumber);

                var response = new PaginatedResponse<Wallet>
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = wallets,
                };

                _logger.LogInformation(
                    "[GetUserWalletsAsync] Wallets retrieved successfully for user: {PhoneNumber}",
                    phoneNumber
                );

                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "Wallets retrieved successfully",
                    data: response
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[GetUserWalletsAsync] An error occurred while getting wallets for user: {PhoneNumber}",
                    phoneNumber
                );

                return new ApiResponse<PaginatedResponse<Wallet>>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while getting wallets for this user"
                );
            }
        }

        public async Task<ApiResponse<Wallet>> GetWalletAsync(string id)
        {
            // check if id is exists and is valid
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("[GetWalletAsync] No wallet id provided.");
                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "No wallet id provided"
                );
            }

            // check if valid mongo id
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("[GetWalletAsync] Invalid wallet id.");
                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid wallet id"
                );
            }

            try
            {
                var wallet = await _walletRepository.GetWalletAsync(id);

                // do not allow users to get wallets that do not belong to them
                if (wallet?.Owner != GetUserPhoneNumber())
                {
                    _logger.LogWarning(
                        "[GetWalletAsync] Unauthorized attempt to get wallet: {WalletId}",
                        id
                    );

                    return new ApiResponse<Wallet>(
                        code: $"{(int)HttpStatusCode.Unauthorized}",
                        message: "Unauthorized attempt to get wallet"
                    );
                }

                if (wallet == null)
                {
                    _logger.LogWarning("[GetWalletAsync] Wallet not found: {WalletId}", id);
                    return new ApiResponse<Wallet>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "Wallet not found"
                    );
                }

                _logger.LogInformation(
                    "[GetWalletAsync] Wallet retrieved successfully: {WalletId}",
                    id
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "Wallet retrieved successfully",
                    data: wallet
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[GetWalletAsync] An error occurred while getting wallet: {WalletId}",
                    id
                );

                return new ApiResponse<Wallet>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while getting wallet"
                );
            }
        }

        public async Task<ApiResponse<bool>> RemoveWalletAsync(string id)
        {
            // check if id is exists and is valid
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("[RemoveWalletAsync] No wallet id provided.");
                return new ApiResponse<bool>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "No wallet id provided"
                );
            }

            // check if valid mongo id
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("[RemoveWalletAsync] Invalid wallet id.");
                return new ApiResponse<bool>(
                    code: $"{(int)HttpStatusCode.NotFound}",
                    message: "Invalid wallet id"
                );
            }

            try
            {
                // check if the authenticated user is the owner of the wallet
                var wallet = await GetWalletAsync(id);

                if (wallet == null)
                {
                    _logger.LogWarning("[RemoveWalletAsync] Wallet not found: {WalletId}", id);
                    return new ApiResponse<bool>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "Wallet not found"
                    );
                }

                if (wallet.Data?.Owner != GetUserPhoneNumber())
                {
                    _logger.LogWarning(
                        "[RemoveWalletAsync] Unauthorized attempt to remove wallet: {WalletId}",
                        id
                    );

                    return new ApiResponse<bool>(
                        code: $"{(int)HttpStatusCode.Unauthorized}",
                        message: "Unauthorized attempt to remove wallet"
                    );
                }

                var result = await _walletRepository.RemoveWalletAsync(id);

                if (!result)
                {
                    _logger.LogWarning("[RemoveWalletAsync] Wallet removal failed: {WalletId}", id);
                    return new ApiResponse<bool>(
                        code: $"{(int)HttpStatusCode.NotFound}",
                        message: "Wallet removal failed"
                    );
                }

                _logger.LogInformation(
                    "[RemoveWalletAsync] Wallet removed successfully: {WalletId}",
                    id
                );
                return new ApiResponse<bool>(
                    code: $"{(int)HttpStatusCode.OK}",
                    message: "Wallet removed successfully"
                );
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[RemoveWalletAsync] An error occurred while removing wallet: {WalletId}",
                    id
                );

                return new ApiResponse<bool>(
                    code: $"{(int)HttpStatusCode.InternalServerError}",
                    message: "An error occurred while removing wallet"
                );
            }
        }

        public async Task<int> GetTotalWalletCountAsync()
        {
            try
            {
                return await _walletRepository.GetTotalWalletCountAsync();
            }
            catch (System.Exception)
            {
                _logger.LogError(
                    "[GetTotalWalletCountAsync] An error occurred while getting total wallet count."
                );
                return 0;
            }
        }
    }
}
