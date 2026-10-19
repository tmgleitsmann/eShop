namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Result-contract parity with legacy/DiscountService/Domain/DiscountEngine.cs:66-73:
/// the id and amount are echoed untouched, DiscountPercent is the raw capped
/// percent, and DiscountedAmount applies the percent to the amount.
/// </summary>
public sealed class DiscountResultContractTests
{
    [Fact]
    public void Result_echoes_inputs_and_reports_percent_and_discounted_amount()
    {
        // The documented legacy example: PREF-0001, 3 years, 250 stacks
        // 3 + 5 + 10 = 18 percent, below the cap; 250 * 82 / 100 = 205.

        //Act
        var result = CalculatorFactory.Default().Calculate("PREF-0001", 250m, 3);

        //Assert
        Assert.Equal("PREF-0001", result.CustomerId);
        Assert.Equal(250m, result.Amount);
        Assert.Equal(18m, result.DiscountPercent);
        Assert.Equal(205m, result.DiscountedAmount);
    }

    [Fact]
    public void Amounts_that_overflow_decimal_throw_like_the_legacy_engine()
    {
        // Legacy ran the same unchecked-against-decimal-limits arithmetic:
        // amount * (100m - percent) overflows decimal and throws. Web API
        // surfaced that as a 500; the migration keeps the throw rather than
        // clamping, so the overflow must still happen.

        //Act - Assert
        Assert.Throws<OverflowException>(() => CalculatorFactory.Default().Calculate("C-001", decimal.MaxValue, 0));
    }
}
