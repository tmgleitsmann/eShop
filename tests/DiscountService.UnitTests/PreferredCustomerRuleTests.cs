namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Preferred-customer parity with legacy/DiscountService/Domain/DiscountEngine.cs:50-53:
/// +5% when a non-null id StartsWith "PREF" with ordinal, case-sensitive
/// comparison. Any string starting with the prefix qualifies, including
/// "PREF" itself; nothing else does.
/// </summary>
public sealed class PreferredCustomerRuleTests
{
    [Theory]
    [InlineData("PREF-0001")]
    [InlineData("PREF")]
    [InlineData("PREF0001")]
    [InlineData("PREFERRED-9")]
    public void Ids_starting_with_the_prefix_earn_the_preferred_bonus(string customerId)
    {
        // Legacy used StartsWith, not equality: exact "PREF" and every longer
        // string prefixed by it (even "PREFERRED-9") earn the bonus.

        //Act
        var result = CalculatorFactory.Default().Calculate(customerId, 50m, 0);

        //Assert
        Assert.Equal(5m, result.DiscountPercent);
        Assert.Equal(47.50m, result.DiscountedAmount);
    }

    [Theory]
    [InlineData("pref-0001")]   // ordinal comparison: lowercase never matches
    [InlineData("Pref-0001")]   // mixed case never matches
    [InlineData("PRE-0001")]    // shorter than the prefix
    [InlineData("XPREF-0001")]  // contains the prefix but is not prefixed by it
    [InlineData("pref")]        // exact lowercase spelling still misses
    [InlineData("")]
    [InlineData("   ")]
    public void Ids_without_the_exact_prefix_earn_no_bonus(string customerId)
    {
        //Act
        var result = CalculatorFactory.Default().Calculate(customerId, 50m, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(50m, result.DiscountedAmount);
    }

    [Fact]
    public void Null_id_earns_no_bonus_and_is_echoed_back_as_null()
    {
        // Legacy null-guards StartsWith (DiscountEngine.cs:50) and echoes the
        // id untouched; the legacy POST endpoint allowed null ids, so results
        // for null ids must keep a null CustomerId.

        //Act
        var result = CalculatorFactory.Default().Calculate(null, 50m, 0);

        //Assert
        Assert.Null(result.CustomerId);
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(50m, result.DiscountedAmount);
    }
}
