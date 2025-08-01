using MetaExchange;
using MetaExchange.Common.Enum;
using MetaExchangeApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var exchangeApiService = new MetaExchangeApiService(new ExchangeService());

app.MapGet("/BTC/{type}/{amount}", IResult (string type, decimal amount) =>
{
    return exchangeApiService.GetBestExecutionPlan(type, amount);
});

app.Run();
