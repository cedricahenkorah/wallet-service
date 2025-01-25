using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WalletService.API.Enums;

namespace WalletService.API.Models;

public class Wallet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WalletType Type { get; set; }

    [BsonElement("accountNumber")]
    public string AccountNumber { get; set; }

    [BsonElement("accountScheme")]
    [BsonRepresentation(BsonType.String)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccountScheme AccountScheme { get; set; }

    public DateTime CreatedAt { get; set; }

    [BsonElement("owner")]
    public string Owner { get; set; }
}
