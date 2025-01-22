using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WalletService.API.Enums;

namespace WalletService.API.Models;

public class Wallet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public WalletType Type { get; set; }
    public string AccountNumber { get; set; }
    public AccountScheme AccountScheme { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Owner { get; set; }
}
