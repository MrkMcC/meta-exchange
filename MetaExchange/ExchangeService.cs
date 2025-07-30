namespace MetaExchange;

using MetaExchange.Common.Exchange;
using MetaExchange.Common.TransactionRecommendation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        foreach (string fileName in Directory.GetFiles("Exchanges", "*.json"))
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

    public PlannedTransaction[] FindBestAsks(decimal BTC)
    {
        List<PlannedTransaction> plannedTransactions = [];
        var remainingBtc = BTC;

        while (remainingBtc > 0)
        {
            var bestTransaction = FindBestAskTransaction(remainingBtc, plannedTransactions);

            plannedTransactions.Add(bestTransaction);
            remainingBtc -= bestTransaction.Amount;
        }

        return [.. plannedTransactions];
    }

    private PlannedTransaction FindBestAskTransaction(decimal amount, params IEnumerable<PlannedTransaction> plannedTransactions)
    {
        return _exchanges.Select(e => FindBestAsk(amount, e, plannedTransactions.Where(t => t.ExchangeId.Equals(e.Id)).Select(t => t.OrderId))).OrderBy(x => x.Price).First();
    }

    private static PlannedTransaction FindBestAsk(decimal amount, Exchange exchange, params IEnumerable<string> excludeOrders)
    {
        var bestAsk = exchange.OrderBook.Asks.Where(a => !excludeOrders.Contains(a.Order.Id)).OrderBy(a => a.Order.Price).First().Order;
        return new PlannedTransaction(exchange.Id, bestAsk, Math.Min(amount, Math.Min(bestAsk.Amount, exchange.AvailableFunds.Crypto)));
    }
}
