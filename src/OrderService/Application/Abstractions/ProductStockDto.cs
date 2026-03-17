namespace OrderService.Application.Abstractions;

public sealed record ProductStockDto(Guid Id, string Name, decimal Price, int Stock);
