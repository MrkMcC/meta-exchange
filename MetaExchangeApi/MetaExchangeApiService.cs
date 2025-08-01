﻿using MetaExchange;
using MetaExchange.Common.Enum;
using MetaExchangeApi.Response;

namespace MetaExchangeApi;

public class MetaExchangeApiService(ExchangeService exchangeService)
{
    private readonly ExchangeService _exchangeService = exchangeService;

    public IResult GetBestExecutionPlan(string type, decimal amountBtc)
    {
        if (!Enum.TryParse<IntendedTransactionType>(type, true, out var transactionType))
        {
            return TypedResults.BadRequest($"Invalid transaction type: '/{type}/'. Try '/buy/' or '/sell/'.");
        }

        var orderType = transactionType == IntendedTransactionType.Buy ? OrderType.Sell : OrderType.Buy;
        var result = _exchangeService.SuggestBestTransactions(orderType, amountBtc);

        if (result.Success)
        {
            return TypedResults.Ok(new BestExecutionPlan
            {
                BestExecution = result.SuggestedTransactions
                .GroupBy(x => x.ExchangeId)
                .Select(g => new ExchangeExecutionPlan
                {
                    ExchangeId = g.Key,
                    Orders = g.Select(t => new OrderExecutionPlan { OrderId = t.OrderId, Amount = t.Amount }).ToArray()
                }).ToArray()
            });
        }
        else if (result.Exception != null)
        {
            return TypedResults.InternalServerError(new { Error = "An internal error occured." });
        }
        else
        {
            return TypedResults.BadRequest(new { Error = result.Message });
        }
    }
}
