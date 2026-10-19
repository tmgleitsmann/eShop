namespace DiscountService;

public sealed class DiscountOptions
{
    public const string SectionName = "DiscountService";

    public decimal LoyaltyYearsPercent { get; set; } = 1m;
    public decimal PreferredCustomerPercent { get; set; } = 5m;
    public string PreferredCustomerPrefix { get; set; } = "PREF";
    public decimal VolumeThreshold { get; set; } = 100m;
    public decimal VolumePercent { get; set; } = 10m;
    public decimal MaxPercent { get; set; } = 20m;
    public int RoundingDigits { get; set; } = 2;
}
