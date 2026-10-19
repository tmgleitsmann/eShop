using System.Text.Json.Serialization;

namespace DiscountService;

public sealed class DiscountResult
{
    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("discountPercent")]
    public decimal DiscountPercent { get; set; }

    [JsonPropertyName("discountedAmount")]
    public decimal DiscountedAmount { get; set; }

    // Preserve the legacy batch audit representation, including current-culture formatting.
    public override string ToString() =>
        CustomerId
        + "|" + Amount.ToString("0.00")
        + "|" + DiscountPercent.ToString("0.0")
        + "|" + DiscountedAmount.ToString("0.00");
}
