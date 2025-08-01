using MetaExchange;
using MetaExchangeApi;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ExchangeService>();
builder.Services.AddSingleton<MetaExchangeApiService>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.EnableAnnotations());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Meta Exchange API");
        options.RoutePrefix = string.Empty;
    });
app.MapOpenApi();

app.MapGet("/BTC/{type}/{amount}", IResult (
    [SwaggerParameter("The type of transaction you would like to perform. 'Buy' if you want to buy BTC, 'Sell' if you want to sell them")] string type,
    [SwaggerParameter("The amount of BTC you want to buy or sell")] decimal amount,
    MetaExchangeApiService exchangeApiService) =>
{
    return exchangeApiService.GetBestExecutionPlan(type, amount);
})
.WithDescription("Finds the best price to sell or buy a given amount of BTC and returns a set of orders to execute against the given exchanges.")
.WithOpenApi();

app.Run();
