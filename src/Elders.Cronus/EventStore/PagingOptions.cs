using System;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Elders.Cronus.EventStore;

public sealed class PagingOptions
{
    PagingOptions() { }

    public PagingOptions(int take, byte[] paginationToken, Order order)
    {
        Take = take;
        PaginationToken = paginationToken;
        Order = order;
    }

    public int Take { get; init; }

    public byte[] PaginationToken { get; init; }

    public Order Order { get; init; }

    public static PagingOptions Empty() => new PagingOptions();

    public override string ToString()
    {
        return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(this));
    }

    public static PagingOptions From(string paginationToken)
    {
        PagingOptions pagingOptions = Empty();
        if (string.IsNullOrEmpty(paginationToken) == false)
        {
            string paginationJson = Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken));
            pagingOptions = JsonSerializer.Deserialize<PagingOptions>(paginationJson);
        }
        return pagingOptions;
    }
}
public sealed class Order : ValueObject<Order>
{
    private const string AscendingOrder = "ascending";
    private const string DescendingOrder = "descending";
    public Order(string order)
    {
        this.order = GetOrderType(order);
    }

    [JsonInclude]
    private readonly string order;

    public static Order Ascending = new Order(AscendingOrder);

    public static Order Descending = new Order(DescendingOrder);

    string GetOrderType(string value)
    {
        if(string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Equals(AscendingOrder, StringComparison.OrdinalIgnoreCase))
            return AscendingOrder;
        else if (value.Equals(DescendingOrder, StringComparison.OrdinalIgnoreCase))
            return DescendingOrder;
        else
            throw new InvalidOperationException("Maimuna");
    }

    public static implicit operator string(Order order)
    {
        if (order is null == true) throw new ArgumentNullException(nameof(order));
        return order.order;
    }
}
