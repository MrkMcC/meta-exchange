namespace MetaExchange;

using MetaExchange.Common.Enum;
using MetaExchange.Common.Exchange;
using MetaExchange.Common.Suggestion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ExchangeService
{
    private readonly Exchange[] _exchanges;
    private readonly JsonSerializerOptions _serializerOptions = new() { Converters = { new JsonStringEnumConverter() } };
    public ExchangeService(Exchange[]? exchanges = null)
    {
        _exchanges = exchanges ?? GetExchangeData();
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
