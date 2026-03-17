namespace OrderService.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found.")
    {
    }
}
