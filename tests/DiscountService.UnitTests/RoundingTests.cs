namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Rounding parity with legacy/DiscountService/Domain/DiscountEngine.cs:71:
/// only the discounted amount is rounded, via decimal.Round(
/// amount * (100m - percent) / 100m, RoundingDigits, MidpointRounding.AwayFromZero);
/// the percent itself is never rounded. Away-from-zero means positive midpoints
/// round up and negative midpoints round further from zero.
/// </summary>
public sealed class RoundingTests
{
    // decimal is not a valid attribute-argument type, so the decimal cases
    // live in MemberData instead of InlineData.
    public static TheoryData<decimal, decimal> PositiveMidpointCases => new()
    {
        { 1.005m, 1.01m },
        { 0.125m, 0.13m },
        { 2.015m, 2.02m },
        { 2.675m, 2.68m },
    };

    public static TheoryData<decimal, decimal> NonMidpointCases => new()
    {
        { 1.004m, 1.00m },
        { 1.006m, 1.01m },
        { -1.004m, -1.00m },
        { -1.006m, -1.01m },
    };

    public static TheoryData<decimal, decimal> ZeroDigitsCases => new()
    {
        { 1.5m, 2m },
        { -1.5m, -2m },
    };

    [Theory]
    [MemberData(nameof(PositiveMidpointCases))]
    public void Positive_midpoints_round_away_from_zero(decimal amount, decimal expectedDiscounted)
    {
        // Zero percent and below-threshold amount: the discounted amount is
        // still passed through decimal.Round, exactly like the legacy engine.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", amount, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(expectedDiscounted, result.DiscountedAmount);
    }

    [Fact]
    public void Negative_midpoints_round_away_from_zero()
    {
        // -1.005 must become -1.01, not -1.00: away from zero, not toward it.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", -1.005m, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(-1.01m, result.DiscountedAmount);
    }

    [Theory]
    [MemberData(nameof(NonMidpointCases))]
    public void Non_midpoints_round_to_the_nearest_cent(decimal amount, decimal expectedDiscounted)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", amount, 0);

        //Assert
        Assert.Equal(expectedDiscounted, result.DiscountedAmount);
    }

    [Fact]
    public void Midpoints_after_a_discount_round_away_from_zero()
    {
        // 100.25 at 10% is 90.225 exactly — a midpoint at the third decimal.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 100.25m, 0);

        //Assert
        Assert.Equal(10m, result.DiscountPercent);
        Assert.Equal(90.23m, result.DiscountedAmount);
    }

    [Fact]
    public void Sub_cent_fractions_below_a_midpoint_round_down()
    {
        // 99.99 at 5% is 94.9905 — below the midpoint, so it becomes 94.99.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 99.99m, 0);

        //Assert
        Assert.Equal(5m, result.DiscountPercent);
        Assert.Equal(94.99m, result.DiscountedAmount);
    }

    [Fact]
    public void Only_the_discounted_amount_is_rounded_the_percent_is_not()
    {
        // With a fractional LoyaltyYearsPercent the legacy engine echoes the
        // raw 0.555% and rounds only the discounted amount:
        // 50 * 99.445 / 100 = 49.7225 -> 49.72.

        //Arrange
        var calculator = CalculatorFactory.With(options => options.LoyaltyYearsPercent = 0.555m);

        //Act
        var result = calculator.Calculate("C-001", 50m, 1);

        //Assert
        Assert.Equal(0.555m, result.DiscountPercent);
        Assert.Equal(49.72m, result.DiscountedAmount);
    }

    [Fact]
    public void Configured_rounding_digits_are_honored()
    {
        // Three digits: 1.0005 is a midpoint at the third decimal -> 1.001.

        //Arrange
        var calculator = CalculatorFactory.With(options => options.RoundingDigits = 3);

        //Act
        var result = calculator.Calculate("C-001", 1.0005m, 0);

        //Assert
        Assert.Equal(1.001m, result.DiscountedAmount);
    }

    [Theory]
    [MemberData(nameof(ZeroDigitsCases))]
    public void Zero_rounding_digits_still_round_away_from_zero(decimal amount, decimal expectedDiscounted)
    {
        //Arrange
        var calculator = CalculatorFactory.With(options => options.RoundingDigits = 0);

        //Act
        var result = calculator.Calculate("C-001", amount, 0);

        //Assert
        Assert.Equal(expectedDiscounted, result.DiscountedAmount);
    }
}
