namespace MetaExchange;

using MetaExchange.Common.Enum;
using MetaExchange.Common.Exchange;
using MetaExchange.Common.Helper;
using MetaExchange.Common.Suggestion;
using System;
using System.Collections.Generic;
using System.Linq;

public class ExchangeService
{
    private readonly Exchange[] _exchanges;

    public ExchangeService(Exchange[]? exchanges = null)
    {
        _exchanges = exchanges ?? ExchangeDataHelper.GetExchangeData();
    }

    public SuggestedTransaction[] SuggestBestTransactions(OrderType orderType, decimal totalAmountBTC)
    {
        List<SuggestedTransaction> suggestedTransactions = [];
        var remainingBtc = totalAmountBTC;

        while (remainingBtc > 0)
        {
            var bestTransaction = SuggestBestTransaction(orderType, remainingBtc, suggestedTransactions);
            suggestedTransactions.Add(bestTransaction);
            remainingBtc -= bestTransaction.Amount;
        }

        return [.. suggestedTransactions];
    }

    private SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, params IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        var suggestions = _exchanges.Select(e => SuggestBestTransaction(orderType, amount, e, suggestedTransactions.Where(t => t.ExchangeId.Equals(e.Id))));

        if (orderType == OrderType.Buy)
            suggestions = suggestions.OrderByDescending(x => x.Price);
        else
            suggestions = suggestions.OrderBy(x => x.Price);

        return suggestions.First();
    }

    private static SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, Exchange exchange, params IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        var orders = GetOrdersByType(exchange, orderType);
        var bestOffer = GetBestOffer(orders.Where(a => !suggestedTransactions.Select(t => t.OrderId).Contains(a.Id)), orderType);
        var remainingFunds = orderType == OrderType.Buy ? CalculateRemainingFundsEuro(exchange, suggestedTransactions) : CalculateRemainingFundsCrypto(exchange, suggestedTransactions);
        return new SuggestedTransaction(exchange.Id, bestOffer, Math.Min(amount, Math.Min(bestOffer.Amount, remainingFunds)));
    }

    private static IEnumerable<Order> GetOrdersByType(Exchange exchange, OrderType orderType)
    {
        var orderBookentries = orderType == OrderType.Buy ? exchange.OrderBook.Bids : exchange.OrderBook.Asks;
        return orderBookentries.Select(e => e.Order).Where(o => o.Type == orderType);
    }

    private static Order GetBestOffer(IEnumerable<Order> orders, OrderType orderType)
    {
        if (orderType == OrderType.Buy)
            orders = orders.OrderByDescending(x => x.Price);
        else
            orders = orders.OrderBy(x => x.Price);

        return orders.First();
    }

    private static decimal CalculateRemainingFundsCrypto(Exchange exchange, IEnumerable<SuggestedTransaction> transactions)
    {
        return exchange.AvailableFunds.Crypto - transactions.Where(t => t.ExchangeId.Equals(exchange.Id) && t.OrderType == OrderType.Sell).Select(t => t.Amount).Sum();
    }

    private static decimal CalculateRemainingFundsEuro(Exchange exchange, IEnumerable<SuggestedTransaction> transactions)
    {
        return exchange.AvailableFunds.Euro - transactions.Where(t => t.ExchangeId.Equals(exchange.Id) && t.OrderType == OrderType.Buy).Select(t => t.Amount * t.Price).Sum();
    }
}
