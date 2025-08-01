using MetaExchange;
using MetaExchange.Common.Enum;
using MetaExchangeApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var exchangeApiService = new MetaExchangeApiService(new ExchangeService());

app.MapGet("/BTC/{type}/{amount}", IResult (string type, decimal amount) =>
{
    if (!Enum.TryParse<IntendedTransactionType>(type, true, out var transactionType))
    {
        return TypedResults.BadRequest($"Invalid transaction type: '/{type}/'. Try '/buy/' or '/sell/'.");
    }

    return TypedResults.Ok(exchangeApiService.GetBestExecutionPlan(transactionType, amount));
});

app.Run();
