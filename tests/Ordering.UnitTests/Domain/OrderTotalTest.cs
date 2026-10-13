namespace eShop.Ordering.UnitTests.Domain;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Locks in the contract of <see cref="Order.GetTotal"/>: each line contributes
/// Units * UnitPrice minus that line's Discount.
/// </summary>
[TestClass]
public class OrderTotalTest
{
    [TestMethod]
    public void Order_total_is_zero_without_items()
    {
        //Arrange
        var order = BuildOrder();

        //Assert
        Assert.AreEqual(0m, order.GetTotal());
    }

    [TestMethod]
    public void Order_total_sums_multiple_undiscounted_lines()
    {
        //Arrange
        var order = BuildOrder((1, 10.0m, 0m), (2, 20.0m, 0m));

        //Assert
        Assert.AreEqual(30.0m, order.GetTotal());
    }

    [TestMethod]
    public void Order_total_subtracts_a_line_level_discount()
    {
        //Arrange
        var order = BuildOrder((1, 12.50m, 2.50m), (2, 7.50m, 0m));

        //Assert
        Assert.AreEqual(17.50m, order.GetTotal());
    }

    [TestMethod]
    public void Order_total_accounts_for_units_and_a_line_discount()
    {
        //Arrange - 4 units at 2.50 with a 0.50 line discount
        var order = BuildOrderWithUnits((1, 2.50m, 0.50m, 4));

        //Assert
        Assert.AreEqual(9.50m, order.GetTotal());
    }

    private static Order BuildOrderWithUnits(params (int ProductId, decimal UnitPrice, decimal Discount, int Units)[] lines)
    {
        var builder = new OrderBuilder(new AddressBuilder().Build());

        foreach (var (productId, unitPrice, discount, units) in lines)
        {
            builder.AddOne(productId, $"product-{productId}", unitPrice, discount, string.Empty, units);
        }

        return builder.Build();
    }

    private static Order BuildOrder(params (int ProductId, decimal UnitPrice, decimal Discount)[] lines)
    {
        var builder = new OrderBuilder(new AddressBuilder().Build());

        foreach (var (productId, unitPrice, discount) in lines)
        {
            builder.AddOne(productId, $"product-{productId}", unitPrice, discount, string.Empty);
        }

        return builder.Build();
    }
}
