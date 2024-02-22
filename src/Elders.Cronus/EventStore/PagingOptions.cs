using System;

namespace Elders.Cronus.EventStore;

public class PagingOptions
{
    PagingOptions() { }

    public PagingOptions(int pageSize, byte[] token, Order order)
    {
        Take = pageSize;
        PaginationToken = token;
        Order = order;
    }

    public int Take { get; init; }

    public byte[] PaginationToken { get; init; }

    public Order Order { get; init; }

    public static PagingOptions Empty() => new PagingOptions();

    public override string ToString()
    {
        return $"\n Records taken: {Take}\n Token: {PaginationToken}";
    }
}

public class Order : ValueObject<Order>
{
    Order() { }

    Order(string order)
    {
        this.order = order;
    }

    private readonly string order;

    public static Order Ascending = new Order("ascending");

    public static Order Descending = new Order("descending");

    public static implicit operator string(Order order)
    {
        if (order is null == true) throw new ArgumentNullException(nameof(order));
        return order.order;
    }
}
