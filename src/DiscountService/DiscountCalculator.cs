using Microsoft.Extensions.Options;

namespace DiscountService;

public sealed class DiscountCalculator(IOptions<DiscountOptions> options)
{
    public DiscountResult Calculate(string? customerId, decimal amount, int loyaltyYears)
    {
        var settings = options.Value;
        decimal percent = settings.LoyaltyYearsPercent * loyaltyYears;

        if (customerId is not null && customerId.StartsWith(settings.PreferredCustomerPrefix, StringComparison.Ordinal))
        {
            percent += settings.PreferredCustomerPercent;
        }

        if (amount >= settings.VolumeThreshold)
        {
            percent += settings.VolumePercent;
        }

        if (percent > settings.MaxPercent)
        {
            percent = settings.MaxPercent;
        }

        return new DiscountResult
        {
            CustomerId = customerId,
            Amount = amount,
            DiscountPercent = percent,
            DiscountedAmount = decimal.Round(amount * (100m - percent) / 100m,
                settings.RoundingDigits, MidpointRounding.AwayFromZero)
        };
    }
}
