namespace DiscountService.UnitTests;

using DiscountService;
using Microsoft.Extensions.Options;

/// <summary>
/// Builds <see cref="DiscountCalculator" /> instances configured like the
/// legacy engine (values from legacy/DiscountService/Web.config appSettings),
/// or with per-rule overrides to pin a single rule at a time.
/// </summary>
internal static class CalculatorFactory
{
    internal static DiscountCalculator Default() => WithOptions(new DiscountOptions());

    internal static DiscountCalculator With(Action<DiscountOptions> configure)
    {
        var options = new DiscountOptions();
        configure(options);
        return WithOptions(options);
    }

    internal static DiscountCalculator WithOptions(DiscountOptions options) =>
        new(Options.Create(options));
}
