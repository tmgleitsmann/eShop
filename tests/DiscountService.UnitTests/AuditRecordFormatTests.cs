namespace DiscountService.UnitTests;

using System.Globalization;
using Xunit;

/// <summary>
/// Audit-record parity with legacy/DiscountService/Domain/DiscountEngine.cs:79
/// and Domain/DiscountResult.cs:19-25: the legacy audit line was the local-time
/// "yyyy-MM-dd HH:mm:ss" timestamp, a space, then the pipe-delimited record
/// CustomerId|Amount(0.00)|DiscountPercent(0.0)|DiscountedAmount(0.00) in the
/// current culture. The migration writes the same record text through ILogger,
/// so <see cref="DiscountResult" /> must keep producing the 2012 batch format
/// field for field.
/// </summary>
public sealed class AuditRecordFormatTests
{
    [Fact]
    public void Audit_record_matches_the_legacy_pipe_delimited_batch_format()
    {
        // 3 + 5 + 10 = 18 percent, 250 * 82 / 100 = 205.
        AssertAuditRecord("PREF-0001", 250m, 3, "PREF-0001|250.00|18.0|205.00");
    }

    [Fact]
    public void Zero_percent_still_prints_one_decimal_place()
    {
        AssertAuditRecord("C-001", 50m, 0, "C-001|50.00|0.0|50.00");
    }

    [Fact]
    public void Null_customer_id_prints_an_empty_first_field()
    {
        // Legacy string concatenation turned null into an empty field, and the
        // legacy POST endpoint allowed null ids, so the 2012 batch import must
        // keep seeing an empty leading field before the first pipe.
        AssertAuditRecord(null, 100m, 0, "|100.00|10.0|90.00");
    }

    [Fact]
    public void Negative_percent_prints_a_leading_minus()
    {
        AssertAuditRecord("C-001", 50m, -3, "C-001|50.00|-3.0|51.50");
    }

    [Fact]
    public void Amount_and_discounted_amount_always_print_two_decimals()
    {
        // 100.5 at 10% is exactly 90.45; the "0.00" format pads the amount.
        AssertAuditRecord("C-001", 100.5m, 0, "C-001|100.50|10.0|90.45");
    }

    [Fact]
    public void Fractional_percent_prints_one_decimal_place()
    {
        //Arrange
        var calculator = CalculatorFactory.With(options => options.LoyaltyYearsPercent = 0.6m);

        //Act
        var result = calculator.Calculate("C-001", 50m, 1);

        //Assert
        Assert.Equal("C-001|50.00|0.6|49.70", FormatInInvariantCulture(result));
    }

    private static void AssertAuditRecord(string? customerId, decimal amount, int years, string expected)
    {
        //Arrange
        var result = CalculatorFactory.Default().Calculate(customerId, amount, years);

        //Assert
        Assert.Equal(expected, FormatInInvariantCulture(result));
    }

    private static string FormatInInvariantCulture(DiscountResult result)
    {
        // Both the legacy engine (.NET Framework on a Windows host) and the
        // migration format with the current culture; the culture itself is not
        // the contract, the format strings are. Pin the invariant culture —
        // effectively the legacy server's — so the format itself is under test.
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            return result.ToString();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
