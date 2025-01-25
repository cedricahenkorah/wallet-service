using MongoDB.Bson.Serialization.Attributes;

namespace WalletService.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
