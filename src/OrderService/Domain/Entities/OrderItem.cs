using OrderService.Domain.Abstractions;

namespace OrderService.Domain.Entities;

public sealed class OrderItem : BaseEntity<Guid>
{
    private OrderItem() { }

    private OrderItem(Guid id, Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        return new OrderItem(Guid.NewGuid(), orderId, productId, productName, unitPrice, quantity);
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal SubTotal => UnitPrice * Quantity;

    public Order Order { get; private set; }

    public void UpdateQuantity(int newQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newQuantity);
        Quantity = newQuantity;
        MarkUpdated();
    }
}
