namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Volume-bonus parity with legacy/DiscountService/Domain/DiscountEngine.cs:55-58
/// and Web.config Discount:VolumeThreshold=100: +10% when amount is greater than
/// OR EQUAL to 100. The threshold is inclusive, and negative amounts never
/// reach it.
/// </summary>
public sealed class VolumeRuleTests
{
    // decimal is not a valid attribute-argument type, so the amount cases
    // live in MemberData instead of InlineData.
    public static TheoryData<decimal, decimal> AboveThresholdCases => new()
    {
        { 100.01m, 90.01m }, // just above: 100.01 * 90 / 100 = 90.009 -> away from zero -> 90.01
        { 250.00m, 225.00m },
        { 1000.00m, 900.00m },
    };

    public static TheoryData<decimal> BelowThresholdCases => new()
    {
        0m,
        -50m,
        -100m, // negative amounts never reach the threshold
    };

    [Fact]
    public void Amount_of_exactly_100_earns_the_volume_bonus()
    {
        // The legacy comparison is >=, not >, so the boundary itself qualifies.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 100m, 0);

        //Assert
        Assert.Equal(10m, result.DiscountPercent);
        Assert.Equal(90m, result.DiscountedAmount);
    }

    [Fact]
    public void Amount_of_99_99_does_not_earn_the_volume_bonus()
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 99.99m, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(99.99m, result.DiscountedAmount);
    }

    [Theory]
    [MemberData(nameof(AboveThresholdCases))]
    public void Amounts_above_the_threshold_earn_ten_percent(decimal amount, decimal expectedDiscounted)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", amount, 0);

        //Assert
        Assert.Equal(10m, result.DiscountPercent);
        Assert.Equal(expectedDiscounted, result.DiscountedAmount);
    }

    [Theory]
    [MemberData(nameof(BelowThresholdCases))]
    public void Amounts_below_the_threshold_earn_no_bonus(decimal amount)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", amount, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(amount, result.DiscountedAmount);
    }
}
