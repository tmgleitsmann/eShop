namespace eShop.Ordering.Domain.Events;

public class CouponAppliedDomainEvent : INotification
{
    public Order Order { get; }

    public string CouponCode { get; }

    public CouponAppliedDomainEvent(Order order, string couponCode)
    {
        Order = order;
        CouponCode = couponCode;
    }
}
