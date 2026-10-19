namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Hard-cap parity with legacy/DiscountService/Domain/DiscountEngine.cs:60-64
/// and Web.config Discount:MaxPercent=20: rules accumulate freely, then the
/// total is clamped only when strictly greater than 20. Exactly 20 stays 20,
/// totals below 20 stay unclamped, and negative totals are never lifted.
/// </summary>
public sealed class HardCapTests
{
    [Fact]
    public void Nineteen_years_alone_stay_just_below_the_cap()
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, 19);

        //Assert
        Assert.Equal(19m, result.DiscountPercent);
        Assert.Equal(40.50m, result.DiscountedAmount);
    }

    [Fact]
    public void Twenty_years_alone_land_exactly_on_the_cap_without_clamping()
    {
        // 20 is not > 20, so legacy leaves the percent untouched.

        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, 20);

        //Assert
        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(40m, result.DiscountedAmount);
    }

    [Theory]
    [InlineData(21)]
    [InlineData(30)]
    [InlineData(100)]
    public void Loyalty_alone_above_the_cap_clamps_to_exactly_twenty(int years)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate("C-001", 50m, years);

        //Assert
        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(40m, result.DiscountedAmount);
    }

    [Fact]
    public void Loyalty_plus_preferred_above_the_cap_clamps_to_twenty()
    {
        // 25 + 5 = 30 raw percent.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 50m, 25);

        //Assert
        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(40m, result.DiscountedAmount);
    }

    [Fact]
    public void All_three_rules_stack_into_exactly_the_cap()
    {
        // 30 loyalty + 5 preferred + 10 volume = 45 raw, then the finance
        // hard cap from 2014 clamps the total to exactly 20.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 250m, 30);

        //Assert
        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(200m, result.DiscountedAmount);
    }

    [Fact]
    public void Rules_stacking_exactly_on_the_cap_are_not_clamped()
    {
        // 5 loyalty + 5 preferred + 10 volume = 20 exactly; not > cap.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 250m, 5);

        //Assert
        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(200m, result.DiscountedAmount);
    }

    [Fact]
    public void Rules_stacking_below_the_cap_stay_unclamped()
    {
        // 2 + 5 + 10 = 17.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 250m, 2);

        //Assert
        Assert.Equal(17m, result.DiscountPercent);
        Assert.Equal(207.50m, result.DiscountedAmount);
    }

    [Fact]
    public void Negative_loyalty_years_cancel_the_preferred_bonus_without_a_lower_cap()
    {
        // -5 + 5 + 0 = 0: legacy clamps only the top of the range, never the bottom.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 50m, -5);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(50m, result.DiscountedAmount);
    }
}
