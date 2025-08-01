namespace MetaExchange.Common.Exchange;

public class Exchange
{
    public required string Id { get; set; }
    public required ExchangeFunds AvailableFunds { get; set; }
    public required OrderBook OrderBook { get; set; }

}
