using OrderService.Domain.Abstractions;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public sealed class Order : BaseEntity<Guid>
{
    private readonly List<OrderItem> _orderItems = new();

    private Order() { }

    private Order(Guid id, Guid customerId, DateTime orderDate)
        : base(id)
    {
        CustomerId = customerId;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
    }

    public static Order Create(Guid customerId)
    {
        return new Order(Guid.NewGuid(), customerId, DateTime.UtcNow);
    }

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public decimal TotalAmount => _orderItems.Sum(item => item.SubTotal);

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Sadece Pending durumundaki siparişlere ürün eklenebilir.");

        var orderItem = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        _orderItems.Add(orderItem);
        MarkUpdated();
    }

    public void RemoveItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Sadece Pending durumundaki siparişlerden ürün çıkarılabilir.");

        var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (item == null)
            throw new InvalidOperationException("Sipariş kalemi bulunamadı.");

        _orderItems.Remove(item);
        MarkUpdated();
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Sadece Pending durumundaki siparişler onaylanabilir.");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Boş sipariş onaylanamaz.");

        Status = OrderStatus.Confirmed;
        MarkUpdated();
    }

    public void ShipOrder()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Sadece Confirmed durumundaki siparişler gönderilebilir.");

        Status = OrderStatus.Shipped;
        MarkUpdated();
    }

    public void DeliverOrder()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Sadece Shipped durumundaki siparişler teslim edilebilir.");

        Status = OrderStatus.Delivered;
        MarkUpdated();
    }
}
