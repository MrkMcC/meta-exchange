namespace MetaExchange.Common.Enum;

//This enum exists separate from OrderType to avoid confusion between the user's intent and the corresponding opposite orders they're looking for.
public enum IntendedTransactionType
{
    Buy = 0,
    Sell = 1
}
