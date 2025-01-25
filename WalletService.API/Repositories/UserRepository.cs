using MongoDB.Driver;
using WalletService.API.Configurations;
using WalletService.API.Models;

namespace WalletService.API.Repositories
{
    public class UserRepository(MongoDbContext mongoDbContext) : IUserRepository
    {
        private readonly IMongoCollection<User> _users = mongoDbContext.Users;

        public async Task AddUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return;
        }

        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _users.Find(user => user.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
        }
    }
}
