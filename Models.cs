using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Erp
{
    public enum OrderStatus
    {
        Created,
        Active,
        Suspended,
        Closed,
        Deleted,
    }

    [DynamoDBTable("Items")]
    public class Item
    {
        [JsonPropertyName("customerCode")]
        [DynamoDBProperty]
        public string? CustomerCode { get; set; }

        [JsonPropertyName("daysToExpiry")]
        [DynamoDBProperty]
        public int? DaysToExpiry { get; set; }

        [JsonPropertyName("defaultShelfLife")]
        [DynamoDBProperty]
        public int? DefaultShelfLife { get; set; }

        [JsonPropertyName("description")]
        [DynamoDBProperty]
        public string? Description { get; set; }

        [JsonPropertyName("extDescription")]
        [DynamoDBProperty]
        public string? ExtDescription { get; set; }

        [JsonPropertyName("height")]
        [DynamoDBProperty]
        public decimal Height { get; set; }

        [JsonPropertyName("length")]
        [DynamoDBProperty]
        public decimal Length { get; set; }

        [JsonPropertyName("name")]
        [DynamoDBHashKey("Name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("netweight")]
        [DynamoDBProperty]
        public decimal NetWeight { get; set; }

        [JsonIgnore]
        [DynamoDBProperty]
        public int PalletsCount { get; set; }

        [JsonPropertyName("quantity")]
        [DynamoDBProperty]
        public decimal Quantity { get; set; }

        [JsonPropertyName("insertDateTime")]
        [DynamoDBProperty]
        public DateTime Version { get; set; }

        [JsonPropertyName("width")]
        [DynamoDBProperty]
        public decimal Width { get; set; }

        [JsonIgnore]
        [DynamoDBProperty]
        public bool Active { get; set; }
    }

    public class ItemEvent
    {
        [JsonPropertyName("versioning")]
        public DateTime Versioning { get; set; }

        [JsonPropertyName("data")]
        public IEnumerable<Item> Data { get; set; } = new List<Item>();
    }

    [DynamoDBTable("Orders")]
    public class Order
    {
        [JsonPropertyName("pallets")]
        [DynamoDBProperty]
        public List<OrderPallet> Pallets { get; set; } = new List<OrderPallet>();

        [JsonPropertyName("number")]
        [DynamoDBHashKey("Number")]
        public string Number { get; set; } = null!;

        [JsonPropertyName("status")]
        [DynamoDBProperty]
        public OrderStatus Status { get; set; }

        [JsonPropertyName("type")]
        [DynamoDBProperty]
        public string Type { get; set; } = null!;
    }

    public class OrderPallet
    {
        [JsonPropertyName("item")]
        public string Item { get; set; } = null!;

        [JsonPropertyName("lpn")]
        public string Lpn { get; set; } = null!;
    }

    public class OrderEvent
    {
        [JsonPropertyName("orders")]
        public IEnumerable<Order> Orders { get; set; }
    }
}