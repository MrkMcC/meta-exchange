namespace MetaExchange.Common.Exchange;

using MetaExchange.Common.Enum;

public class Order
{
    public required string Id { get; set; }
    public DateTime Time { get; set; }
    public OrderType Type { get; set; }
    public OrderKind Kind { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
}
