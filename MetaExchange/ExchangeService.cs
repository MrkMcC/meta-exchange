namespace MetaExchange;

using MetaExchange.Common.Enum;
using MetaExchange.Common.Exchange;
using MetaExchange.Common.Suggestion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ExchangeService
{
    private readonly Exchange[] _exchanges;
    private readonly JsonSerializerOptions _serializerOptions = new() { Converters = { new JsonStringEnumConverter() } };
    public ExchangeService()
    {
        _exchanges = GetExchangeData();
    }

    private Exchange[] GetExchangeData()
    {
        var exchanges = new List<Exchange>();
        var executingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        foreach (string fileName in Directory.GetFiles($"{executingPath}/Exchanges", "*.json"))
        {
            using Stream stream = new FileStream(fileName, FileMode.Open);
            if (File.Exists(fileName) && stream.Length > 0)
            {
                using StreamReader reader = new(stream);
                string fileContents = reader.ReadToEnd();
                var exchange = JsonSerializer.Deserialize<Exchange>(fileContents, _serializerOptions);
                if (exchange != null)
                {
                    exchanges.Add(exchange);
                }
            }
        }
        return [.. exchanges];
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

    private SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, params IEnumerable<SuggestedTransaction> plannedTransactions)
    {
        var suggestions = _exchanges.Select(e => SuggestBestTransaction(orderType, amount, e, plannedTransactions.Where(t => t.ExchangeId.Equals(e.Id)).Select(t => t.OrderId)));

        if (orderType == OrderType.Buy)
            suggestions = suggestions.OrderByDescending(x => x.Price);
        else
            suggestions = suggestions.OrderBy(x => x.Price);

        return suggestions.First();
    }

    private static SuggestedTransaction SuggestBestTransaction(OrderType orderType, decimal amount, Exchange exchange, params IEnumerable<string> excludeOrders)
    {
        var orders = GetOrdersByType(exchange, orderType);
        var bestOffer = GetBestOffer(orders.Where(a => !excludeOrders.Contains(a.Id)), orderType);
        return new SuggestedTransaction(exchange.Id, bestOffer, Math.Min(amount, Math.Min(bestOffer.Amount, exchange.AvailableFunds.Crypto)));
    }

    private static IEnumerable<Order> GetOrdersByType(Exchange exchange, OrderType orderType)
    {
        var orderBookentries = orderType == OrderType.Buy ? exchange.OrderBook.Bids : exchange.OrderBook.Asks;
        return orderBookentries.Select(e => e.Order);
    }

    private static Order GetBestOffer(IEnumerable<Order> orders, OrderType orderType)
    {
        orders = orders.Where(o => o.Type == orderType);

        if (orderType == OrderType.Buy)
            orders = orders.OrderByDescending(x => x.Price);
        else
            orders = orders.OrderBy(x => x.Price);

        return orders.First();
    }
}
