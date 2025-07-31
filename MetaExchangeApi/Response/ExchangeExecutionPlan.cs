namespace MetaExchangeApi.Response;

public class ExchangeExecutionPlan
{
    public string? ExchangeId { get; set; }
    public OrderExecutionPlan[] Orders { get; set; } = [];
}
