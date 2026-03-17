namespace OrderService.Domain.Exceptions;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Product '{productId}' has insufficient stock. Requested: {requested}, Available: {available}.")
    {
    }
}
