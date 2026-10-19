namespace DiscountService.UnitTests;

using Xunit;

/// <summary>
/// Options parity: every <see cref="DiscountOptions" /> default matches the
/// value the legacy engine read from legacy/DiscountService/Web.config
/// appSettings, and configured values drive every rule the same way
/// ConfigurationManager values did.
/// </summary>
public sealed class DiscountOptionsTests
{
    [Fact]
    public void Defaults_match_the_legacy_web_config()
    {
        //Arrange
        var options = new DiscountOptions();

        //Assert
        Assert.Equal(1m, options.LoyaltyYearsPercent);        // Discount:LoyaltyYearsPercent
        Assert.Equal(5m, options.PreferredCustomerPercent);   // Discount:PreferredCustomerPercent
        Assert.Equal("PREF", options.PreferredCustomerPrefix); // hardcoded ordinal prefix
        Assert.Equal(100m, options.VolumeThreshold);           // Discount:VolumeThreshold
        Assert.Equal(10m, options.VolumePercent);              // Discount:VolumePercent
        Assert.Equal(20m, options.MaxPercent);                 // Discount:MaxPercent
        Assert.Equal(2, options.RoundingDigits);               // hardcoded decimal.Round digits
    }

    [Fact]
    public void Configured_values_drive_every_rule()
    {
        //Arrange
        var calculator = CalculatorFactory.With(options =>
        {
            options.LoyaltyYearsPercent = 2m;
            options.PreferredCustomerPercent = 7m;
            options.PreferredCustomerPrefix = "VIP";
            options.VolumeThreshold = 50m;
            options.VolumePercent = 15m;
            options.MaxPercent = 25m;
        });

        //Act
        // 3*2 loyalty + 7 preferred + 15 volume = 28 raw -> capped at 25;
        // 60 * 75 / 100 = 45.
        var result = calculator.Calculate("VIP-9", 60m, 3);

        //Assert
        Assert.Equal(25m, result.DiscountPercent);
        Assert.Equal(45m, result.DiscountedAmount);
    }

    [Fact]
    public void Configured_prefix_stays_ordinal_case_sensitive()
    {
        //Arrange
        var calculator = CalculatorFactory.With(options => options.PreferredCustomerPrefix = "VIP");

        //Act
        var result = calculator.Calculate("vip-9", 60m, 0);

        //Assert
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(60m, result.DiscountedAmount);
    }

    [Fact]
    public void Rounding_digits_outside_the_decimal_range_throw_when_used()
    {
        // decimal.Round only accepts 0..28 digits; a misconfigured value must
        // fail the calculation loudly, the way a bad appSetting did on legacy
        // cold start (legacy/DiscountService/Domain/DiscountEngine.cs:32-40).

        //Arrange
        var calculator = CalculatorFactory.With(options => options.RoundingDigits = 29);

        //Act - Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate("C-001", 10m, 0));
    }
}
