namespace MetaExchangeApi.Response;

public class BestExecutionPlan
{
    public string? Message { get; set; }
    public ExchangeExecutionPlan[] BestExecution { get; set; } = [];
}
