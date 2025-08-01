namespace MetaExchange.Common.Suggestion;

public class SuggestionResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public SuggestedTransaction[]? SuggestedTransactions { get; set; }
}
