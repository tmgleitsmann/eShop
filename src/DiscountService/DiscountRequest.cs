using System.Text.Json.Serialization;

namespace DiscountService;

public sealed class DiscountRequest
{
    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("loyaltyYears")]
    public int LoyaltyYears { get; set; }
}
