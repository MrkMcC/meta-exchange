namespace MetaExchange.Common.Exchange;

public class OrderBook
{
    public required OrderBookEntry[] Bids { get; set; }
    public required OrderBookEntry[] Asks { get; set; }
}
