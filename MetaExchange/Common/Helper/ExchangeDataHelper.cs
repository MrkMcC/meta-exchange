namespace MetaExchange.Common.Helper;

using MetaExchange.Common.Exchange;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ExchangeDataHelper
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { Converters = { new JsonStringEnumConverter() } };

    public static Exchange[] GetExchangeData()
    {
        var exchanges = new List<Exchange>();
        var executingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        foreach (string fileName in Directory.GetFiles($"{executingPath}/Exchanges", "*.json"))
        {
            var exchange = GetExchangeDataFromFile(fileName);
            if (exchange != null)
            {
                exchanges.Add(exchange);
            }
        }
        return [.. exchanges];
    }

    public static Exchange GetExchangeDataFromFile(string fileName)
    {
        using Stream stream = new FileStream(fileName, FileMode.Open);
        if (File.Exists(fileName) && stream.Length > 0)
        {
            using StreamReader reader = new(stream);
            string fileContents = reader.ReadToEnd();
            var exchange = JsonSerializer.Deserialize<Exchange>(fileContents, _serializerOptions);
            return exchange;
        }

        throw new FileNotFoundException($"Could not find exchange data file: {fileName}");
    }
}
