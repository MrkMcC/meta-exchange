using MetaExchange;
using MetaExchange.Common.Enum;
using MetaExchangeApi.Response;

namespace MetaExchangeApi;

public class MetaExchangeApiService(ExchangeService exchangeService)
{
    private readonly ExchangeService _exchangeService = exchangeService;

    public BestExecutionPlan GetBestExecutionPlan(IntendedTransactionType transactionType, decimal amountBtc)
    {
        var orderType = transactionType == IntendedTransactionType.Buy ? OrderType.Sell : OrderType.Buy;
        var suggestedTransactions = _exchangeService.SuggestBestTransactions(orderType, amountBtc);

        return new BestExecutionPlan
        {
            ExecutionPlan = [.. suggestedTransactions
                .Select(t => t.ExchangeId)
                .Distinct()
                .Select(id => new ExchangeExecutionPlan
                    {
                    ExchangeId = id,
                    Orders = [.. suggestedTransactions
                        .Where(t => t.ExchangeId.Equals(id))
                        .Select(t => new OrderExecutionPlan { OrderId = t.OrderId, Amount = t.Amount })
                        ]
                    })
                ]
        };
    }
}
