using ProductService.Domain.Abstractions;

namespace ProductService.Domain.Entities;

public sealed class Product : BaseEntity<Guid>
{
    // 2. EF Constructor
    private Product() { }

    // 3. Primary Constructor
    private Product(Guid id, string name, decimal price, int stock, string sku)
        : base(id)
    {
        Name = name;
        Price = price;
        Stock = stock;
        SKU = sku;
    }

    // 4. Factory Method
    public static Product Create(string name, decimal price, int stock, string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stock);

        return new Product(Guid.NewGuid(), name, price, stock, sku);
    }

    // 5. Properties
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string SKU { get; private set; }

    // 6. Domain Methods
    public void UpdatePrice(decimal newPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newPrice);
        Price = newPrice;
        MarkUpdated();
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity > Stock)
            throw new InvalidOperationException("Yetersiz stok.");

        Stock -= quantity;
        MarkUpdated();
    }

    public void IncreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        Stock += quantity;
        MarkUpdated();
    }

    public void Update(string name, decimal price, int stock, string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stock);

        Name = name;
        Price = price;
        Stock = stock;
        SKU = sku;
        MarkUpdated();
    }
}
