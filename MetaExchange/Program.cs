using MetaExchange;
using MetaExchange.Common.Enum;
using System.Globalization;

Console.WriteLine("Starting MetaExchange Application...");
Console.WriteLine("");

Console.WriteLine("Buy or sell BTC?");
Console.WriteLine("0 = Buy");
Console.WriteLine("1 = Sell");
IntendedTransactionType intendedTransactionType;
var inputOrderType = Console.ReadLine();
while (!Enum.TryParse<IntendedTransactionType>(inputOrderType, true, out intendedTransactionType) || !Enum.IsDefined<IntendedTransactionType>(intendedTransactionType))
{
    Console.WriteLine($"'{inputOrderType}' is not a valid input. Please input '0', '1', 'buy' or 'sell'.");
    inputOrderType = Console.ReadLine();
}

Console.WriteLine($"{intendedTransactionType} how many BTC? (Use '{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}' as a decimal separator.)");
var inputOrderAmount = Console.ReadLine();

decimal amount;
while (!decimal.TryParse(inputOrderAmount, CultureInfo.CurrentCulture, out amount))
{
    Console.WriteLine($"'{inputOrderAmount}' is not a valid amount. Please enter a decimal value.");
    inputOrderAmount = Console.ReadLine();
}

Console.WriteLine();
Console.WriteLine($"Finding the best orders to {intendedTransactionType} {amount} BTC...");
Console.WriteLine();

var exchangeService = new ExchangeService();
var orderType = intendedTransactionType == IntendedTransactionType.Buy ? OrderType.Sell : OrderType.Buy;
var result = exchangeService.SuggestBestTransactions(orderType, amount);

if (result.Success)
{
    Console.WriteLine("Here is the suggested execution plan:");
    foreach (var item in result.SuggestedTransactions)
        Console.WriteLine($"{intendedTransactionType} {item.Amount} BTC at {item.Price} Euro each | Exchange: {item.ExchangeId} Order: {item.OrderId}");
}
else
{
    Console.WriteLine("Could not fulfil the request.");
    if (result.Exception != null)
    {
        Console.WriteLine(result.Exception.GetType());
        Console.WriteLine(result.Exception.Message);
    }
}

if (result.Message != null)
{
    Console.WriteLine(result.Message);
}

Console.WriteLine();
Console.WriteLine("Terminating MetaExchange...");