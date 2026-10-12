namespace eShop.Ordering.UnitTests.Domain;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

[TestClass]
public class CouponTest
{
    [TestMethod]
    public void Create_coupon_success()
    {
        //Act
        var coupon = new Coupon("save10", 0.10m, minimumBasketTotal: 20.0m);

        //Assert
        Assert.AreEqual("SAVE10", coupon.Code);
        Assert.AreEqual(0.10m, coupon.Percentage);
        Assert.AreEqual(20.0m, coupon.MinimumBasketTotal);
    }

    [TestMethod]
    public void Invalid_coupon_code()
    {
        Assert.ThrowsExactly<OrderingDomainException>(() => new Coupon("  ", 0.10m));
    }

    [TestMethod]
    public void Invalid_coupon_percentage()
    {
        Assert.ThrowsExactly<OrderingDomainException>(() => new Coupon("SAVE10", 0m));
        Assert.ThrowsExactly<OrderingDomainException>(() => new Coupon("SAVE10", 10m));
    }

    [TestMethod]
    public void Apply_coupon_reduces_the_total_by_the_coupon_percentage()
    {
        //Arrange
        var order = BuildOrder((1, 10.0m, 0m), (2, 20.0m, 0m));

        //Act
        order.ApplyCoupon(new Coupon("SAVE10", 0.10m));

        //Assert
        Assert.AreEqual(27.0m, order.GetTotal());
        Assert.HasCount(2, order.DomainEvents);
    }

    [TestMethod]
    public void Order_total_takes_line_level_discounts_into_account()
    {
        //Arrange
        var order = BuildOrder((1, 10.0m, 2.0m), (2, 20.0m, 0m));

        //Assert
        Assert.AreEqual(28.0m, order.GetTotal());
    }

    [TestMethod]
    public void Apply_coupon_below_the_minimum_basket_total()
    {
        //Arrange
        var order = BuildOrder((1, 10.0m, 0m));

        //Act - Assert
        Assert.ThrowsExactly<OrderingDomainException>(() => order.ApplyCoupon(new Coupon("BIGSPENDER", 0.05m, minimumBasketTotal: 50.0m)));
    }

    [TestMethod]
    public void Apply_coupon_twice_on_the_same_order()
    {
        //Arrange
        var order = BuildOrder((1, 10.0m, 0m));
        order.ApplyCoupon(new Coupon("SAVE10", 0.10m));

        //Act - Assert
        Assert.ThrowsExactly<OrderingDomainException>(() => order.ApplyCoupon(new Coupon("SAVE10", 0.10m)));
    }

    [TestMethod]
    public void Apply_coupon_to_an_order_without_items()
    {
        //Arrange
        var order = BuildOrder();

        //Act - Assert
        Assert.ThrowsExactly<OrderingDomainException>(() => order.ApplyCoupon(new Coupon("SAVE10", 0.10m)));
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
