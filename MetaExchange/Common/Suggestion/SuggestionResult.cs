namespace MetaExchange.Common.Suggestion;

public class SuggestionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public Exception? Exception { get; set; }

    public SuggestedTransaction[]? SuggestedTransactions { get; set; }
}
