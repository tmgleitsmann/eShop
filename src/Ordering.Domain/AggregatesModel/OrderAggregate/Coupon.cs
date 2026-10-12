using eShop.Ordering.Domain.SeedWork;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Basket level promotional coupon. A coupon grants a percentage off the basket
/// and may require a minimum basket total before it can be redeemed.
/// </summary>
public class Coupon
    : ValueObject
{
    public string Code { get; }

    /// <summary>
    /// Fraction of the basket granted by the coupon, e.g. 0.10m for 10% off.
    /// </summary>
    public decimal Percentage { get; }

    public decimal MinimumBasketTotal { get; }

    protected Coupon() { }

    public Coupon(string code, decimal percentage, decimal minimumBasketTotal = 0m)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new OrderingDomainException("Coupon code is required");
        }

        if (percentage <= 0m || percentage >= 1m)
        {
            throw new OrderingDomainException("Coupon percentage must be greater than 0 and lower than 1");
        }

        if (minimumBasketTotal < 0m)
        {
            throw new OrderingDomainException("Coupon minimum basket total cannot be negative");
        }

        Code = code.Trim().ToUpperInvariant();
        Percentage = percentage;
        MinimumBasketTotal = minimumBasketTotal;
    }

    public bool IsApplicableTo(decimal basketTotal) => basketTotal >= MinimumBasketTotal;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Percentage;
        yield return MinimumBasketTotal;
    }
}
