namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Loyalty-rule parity with legacy/DiscountService/Domain/DiscountEngine.cs:48:
/// percent starts at LoyaltyYearsPercent * loyaltyYears, with no per-rule cap,
/// no lower bound, and no validation of loyaltyYears.
/// </summary>
public sealed class LoyaltyRuleTests
{
    // decimal is not a valid attribute-argument type, so the per-year cases
    // live in MemberData instead of InlineData.
    public static TheoryData<int, decimal, decimal> PerYearCases => new()
    {
        { 1, 1m, 49.50m },
        { 2, 2m, 49.00m },
        { 3, 3m, 48.50m },
        { 5, 5m, 47.50m },
        { 9, 9m, 45.50m },
    };

    [Fact]
    public void Zero_loyalty_years_earn_no_discount()
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(50m, result.DiscountedAmount);
    }

    [Theory]
    [MemberData(nameof(PerYearCases))]
    public void Loyalty_accrues_one_percent_per_year(int years, decimal expectedPercent, decimal expectedDiscounted)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, years);

        //Assert
        Assert.Equal(expectedPercent, result.DiscountPercent);
        Assert.Equal(expectedDiscounted, result.DiscountedAmount);
    }

    [Fact]
    public void Fifteen_years_earn_fifteen_percent_despite_the_old_cap()
    {
        // The legacy engine caps only the total, and only above 20; a single
        // rule accruing below the cap is never trimmed on its own.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, 15);

        //Assert
        Assert.Equal(15m, result.DiscountPercent);
        Assert.Equal(42.50m, result.DiscountedAmount);
    }

    [Fact]
    public void Negative_loyalty_years_produce_a_negative_percent_without_a_lower_cap()
    {
        // Legacy guards nothing: -3 years yield -3%, which raises the discounted
        // amount, and the >20 clamp never touches a negative percent.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, -3);

        //Assert
        Assert.Equal(-3m, result.DiscountPercent);
        Assert.Equal(51.50m, result.DiscountedAmount);
    }
}
