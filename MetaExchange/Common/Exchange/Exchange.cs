namespace MetaExchange.Common.Exchange;

public class Exchange
{
    public string? Id { get; set; }
    public ExchangeFunds? AvailableFunds { get; set; }
    public OrderBook? OrderBook { get; set; }

}
