using MetaExchange.Common.Enum;
using MetaExchange.Common.Exchange;
using MetaExchange.Common.Helper;
using MetaExchange.Common.Suggestion;
using System.Reflection;

namespace MetaExchange.Tests;

public class ExchangeService_Tests
{
    private readonly Exchange[] _defaultExchangeData = ExchangeDataHelper.GetExchangeData();
    private readonly string _executingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

    private static void AssertSuccess(SuggestionResult result)
    {
        Assert.True(result.Success);
        Assert.Null(result.Exception);
        Assert.NotEmpty(result.SuggestedTransactions);
    }

    private static void AssertHandledError(SuggestionResult result)
    {
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public void Can_Buy_1_Bitcoin()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);
        var result = exchangeService.SuggestBestTransactions(OrderType.Sell, 1);
        AssertSuccess(result);
    }

    [Fact]
    public void Can_Sell_1_Bitcoin()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);
        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, 1);
        AssertSuccess(result);
    }

    [Fact]
    public void Can_Sell_Partial_Bitcoin()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);
        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, 0.128m);
        AssertSuccess(result);
    }

    [Fact]
    public void Can_Not_Buy_0_Bitcoin()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);
        var result = exchangeService.SuggestBestTransactions(OrderType.Sell, 0);
        AssertHandledError(result);
    }

    [Fact]
    public void Can_Not_Sell_Negative_Bitcoin()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);
        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, -1);
        AssertHandledError(result);
    }

    [Fact]
    public void ResultAmount_Equals_InputAmount()
    {
        const decimal amount = (decimal)12.345;
        var exchangeService = new ExchangeService(_defaultExchangeData);

        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, amount);
        AssertSuccess(result);
        var resultAmountBtc = result.SuggestedTransactions.Select(t => t.Amount).Sum();

        Assert.Equal(amount, resultAmountBtc);
    }

    [Fact]
    public void BuyOrders_GreaterPriceThan_SellOrders()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);

        var buyResult = exchangeService.SuggestBestTransactions(OrderType.Buy, 0.1m);
        var sellResult = exchangeService.SuggestBestTransactions(OrderType.Sell, 0.1m);

        AssertSuccess(buyResult);
        Assert.True(buyResult.SuggestedTransactions.First().Price < sellResult.SuggestedTransactions.First().Price, "The best 'buy' orders should still have a lower price than the best 'sell' orders.");
    }

    [Fact]
    public void IDs_Not_NullOrEmpty()
    {
        var exchangeService = new ExchangeService(_defaultExchangeData);

        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, 1);

        AssertSuccess(result);
        Assert.False(result.SuggestedTransactions.Any(t => string.IsNullOrEmpty(t.ExchangeId)), "All results should have an ExchangeId.");
        Assert.False(result.SuggestedTransactions.Any(t => string.IsNullOrEmpty(t.OrderId)), "All results should have an OrderId.");
    }

    [Fact]
    public void Exchange_Orders_May_Deplete()
    {
        var exchangeService = new ExchangeService([
            ExchangeDataHelper.GetExchangeDataFromFile($"{_executingPath}/TestData/exchange-01.json"),
            ExchangeDataHelper.GetExchangeDataFromFile($"{_executingPath}/TestData/exchange_single-best-order.json")]);

        var resultB = exchangeService.SuggestBestTransactions(OrderType.Sell, 1);
        AssertSuccess(resultB);
        Assert.Single(resultB.SuggestedTransactions.Where(t => t.ExchangeId.Equals($"exchange_single-best-order")).ToArray());
    }

    [Fact]
    public void Exceeding_Funds_Returns_Error()
    {
        var exchangeService = new ExchangeService([ExchangeDataHelper.GetExchangeDataFromFile($"{_executingPath}/TestData/exchange_fund-limit.json")]);
        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, 0.5m);
        AssertHandledError(result);
        Assert.Contains("funds", result.Message);
    }

    [Fact]
    public void Exceeding_Availability_Returns_Error()
    {
        var exchangeService = new ExchangeService([ExchangeDataHelper.GetExchangeDataFromFile($"{_executingPath}/TestData/exchange_single-best-order.json")]);
        var result = exchangeService.SuggestBestTransactions(OrderType.Buy, 1);
        AssertHandledError(result);
        Assert.Contains("orders", result.Message);
    }
}
