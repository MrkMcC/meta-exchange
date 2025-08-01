namespace MetaExchange;

using MetaExchange.Common.Enum;
using MetaExchange.Common.Exceptions;
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

    public SuggestionResult SuggestBestTransactions(OrderType orderType, decimal totalAmountBTC)
    {
        try
        {
            List<SuggestedTransaction> suggestedTransactions = [];
            var remainingBtc = totalAmountBTC;

            while (remainingBtc > 0)
            {
                AssertAvailability(orderType, suggestedTransactions, totalAmountBTC - remainingBtc);
                var bestTransaction = SuggestBestTransaction(orderType, remainingBtc, suggestedTransactions);
                suggestedTransactions.Add(bestTransaction);
                remainingBtc -= bestTransaction.Amount;
            }

            return new SuggestionResult { Success = true, SuggestedTransactions = [.. suggestedTransactions] };
        }
        catch (MetaExchangeException ex)
        {
            return new SuggestionResult { Success = false, Message = ex.Message };
        }
        catch (Exception ex)
        {
            return new SuggestionResult { Success = false, Exception = ex };
        }
    }

    private void AssertAvailability(OrderType orderType, IEnumerable<SuggestedTransaction> suggestedTransactions, decimal spentBtc)
    {
        var validAmount = $"{Math.Floor(spentBtc * 1000) / 1000:#.###}";
        var exchangesWithFunds = _exchanges.Where(e => HasRemainingFunds(e, orderType, suggestedTransactions));
        if (!exchangesWithFunds.Any())
        {
            throw new MetaExchangeException($"The total available funds of all exchanges are insufficient to fulfil this request. The limit was reached at {validAmount} BTC.");
        }
        var availableExchanges = _exchanges.Where(e => HasRemainingOrders(e, orderType, suggestedTransactions));
        if (!availableExchanges.Any())
        {
            throw new MetaExchangeException($"There are not enough orders available to fulfil this request. The limit was reached at {validAmount} BTC.");
        }
    }

    private SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, params IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        var availableExchanges = _exchanges.Where(e => HasRemainingFunds(e, orderType, suggestedTransactions) && HasRemainingOrders(e, orderType, suggestedTransactions));
        var suggestions = availableExchanges.Select(e => SuggestBestTransaction(orderType, amount, e, suggestedTransactions.Where(t => t.ExchangeId.Equals(e.Id))));
        suggestions = suggestions.Where(s => s != null);

        if (orderType == OrderType.Buy)
            suggestions = suggestions.OrderByDescending(x => x.Price);
        else
            suggestions = suggestions.OrderBy(x => x.Price);

        return suggestions.First();
    }

    private static SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, Exchange exchange, params IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        var orders = GetOrdersByType(exchange, orderType);
        var bestOffer = GetBestOffer(GetRemainingOrders(orders, suggestedTransactions), orderType);

        decimal btcFundLimit = orderType == OrderType.Buy ? CalculateRemainingFundsEuro(exchange, suggestedTransactions) / bestOffer.Price : CalculateRemainingFundsCrypto(exchange, suggestedTransactions);

        var suggestedAmount = Math.Min(amount, Math.Min(bestOffer.Amount, btcFundLimit));
        return new SuggestedTransaction(exchange.Id, bestOffer, suggestedAmount);
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

    private static IEnumerable<Order> GetRemainingOrders(IEnumerable<Order> orders, IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        return orders.Where(o => !suggestedTransactions.Select(t => t.OrderId).Contains(o.Id));
    }

    private static bool HasRemainingOrders(Exchange exchange, OrderType orderType, IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        return GetRemainingOrders(GetOrdersByType(exchange, orderType), suggestedTransactions).Any();
    }

    private static bool HasRemainingFunds(Exchange exchange, OrderType orderType, IEnumerable<SuggestedTransaction> suggestedTransactions)
    {
        if (orderType == OrderType.Buy)
            return CalculateRemainingFundsEuro(exchange, suggestedTransactions) > 0;
        else
            return CalculateRemainingFundsCrypto(exchange, suggestedTransactions) > 0;
    }
}
