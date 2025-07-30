using MetaExchange.Common.Exchange;

namespace MetaExchange.Common.TransactionRecommendation;

public class PlannedTransaction(string exchangeId, Order order, decimal amount)
{
    public readonly string ExchangeId = exchangeId;
    public readonly string OrderId = order.Id;
    public readonly decimal Price = order.Price;
    public readonly decimal Amount = amount;
}
