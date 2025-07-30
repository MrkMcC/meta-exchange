using MetaExchange;
using MetaExchange.Common.Enum;
using System.Globalization;

Console.WriteLine("Starting MetaExchange Application...");
Console.WriteLine("");

//Console.WriteLine("Provide the order type (buy/sell):");
//var inputOrderType = Console.ReadLine();
var orderType = OrderType.Buy;


Console.WriteLine($"{orderType} how many BTC? (Use '{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}' as a decimal separator.)");
var inputOrderAmount = Console.ReadLine();

decimal amount;
while (!decimal.TryParse(inputOrderAmount, CultureInfo.CurrentCulture, out amount))
{
    Console.WriteLine($"'{inputOrderAmount}' is not a valid amount. Please enter a decimal value.");
    inputOrderAmount = Console.ReadLine();
}

Console.WriteLine();
Console.WriteLine($"Finding the best orders to {orderType} {amount} BTC...");

var exchangeService = new ExchangeService();
var result = exchangeService.FindBestAsks(amount);

Console.WriteLine();
Console.WriteLine("Here is the suggested execution plan:");
foreach (var item in result)
    Console.WriteLine($"{orderType} {item.Amount} BTC at {item.Price} Euro each | Exchange: {item.ExchangeId} Order: {item.OrderId}");

Console.WriteLine();
Console.WriteLine("Terminating MetaExchange...");