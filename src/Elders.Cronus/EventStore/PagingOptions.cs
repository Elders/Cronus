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

    public string Serialize()
    {
        return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(this));
    }

    public static PagingOptions Deserialize(string paginationToken)
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
    Order() { }

    Order(string order)
    {
        this.order = order;
    }

    [JsonInclude]
    private readonly string order;

    public static Order Ascending = new Order("ascending");

    public static Order Descending = new Order("descending");

    public static implicit operator string(Order order)
    {
        if (order is null == true) throw new ArgumentNullException(nameof(order));
        return order.order;
    }
}
