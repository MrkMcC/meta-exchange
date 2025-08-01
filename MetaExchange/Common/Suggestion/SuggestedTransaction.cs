using MetaExchange.Common.Enum;
using MetaExchange.Common.Exchange;

namespace MetaExchange.Common.Suggestion;

public class SuggestedTransaction
{
    public SuggestedTransaction(string exchangeId, Order order, decimal amount)
    {
        ArgumentException.ThrowIfNullOrEmpty(exchangeId);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        ExchangeId = exchangeId;
        OrderId = order.Id;
        OrderType = order.Type;
        Price = order.Price;
        Amount = amount;
    }

    public readonly string ExchangeId;
    public readonly string OrderId;
    public readonly OrderType OrderType;
    public readonly decimal Price;
    public readonly decimal Amount;
}
