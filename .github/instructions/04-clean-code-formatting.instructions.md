---
name: Clean Code - Biçimlendirme
description: Robert C. Martin 'Clean Code' Chapter 5 — Formatting. C# dosyalarında dikey ve yatay biçimlendirme kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Biçimlendirme (Chapter 5)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 5.

**Temel prensip:** Biçimlendirme iletişim aracıdır. Kodun bugünkü çalışmasından çok, sürdürülebilirliği için okunabilirlik kritiktir.

---

## 1. Dikey Biçimlendirme

### 1.1 Kavramlar Arası Dikey Boşluk

Birbirinden farklı kavramlar (import blokları, field'lar, metodlar) arasına boş satır bırak.

```csharp
// YANLIS — bosluklarla ayrilmamis
public sealed class ProductRepository : IProductRepository
{
    private readonly ProductServiceDbContext _context;
    public ProductRepository(ProductServiceDbContext context)
    {
        _context = context;
    }
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Products.FindAsync(id, ct);
    public async Task AddAsync(Product product, CancellationToken ct)
        => await _context.Products.AddAsync(product, ct);
}

// DOGRU — kavramlar dikey boslukla ayrildi
public sealed class ProductRepository : IProductRepository
{
    private readonly ProductServiceDbContext _context;

    public ProductRepository(ProductServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Products.FindAsync(id, ct);

    public async Task AddAsync(Product product, CancellationToken ct)
        => await _context.Products.AddAsync(product, ct);
}
```

### 1.2 Dikey Yoğunluk (İlgili Satırlar Yakın Olmalı)

Birbirine ait satırlar arasına gereksiz boş satır bırakma.

```csharp
// YANLIS — ilgili satirlar gereksiz boslukla ayrilmis
private readonly IProductRepository _productRepository;

private readonly IUnitOfWork _unitOfWork;

// DOGRU — ilgili field'lar beraber
private readonly IProductRepository _productRepository;
private readonly IUnitOfWork _unitOfWork;
```

### 1.3 Dikey Mesafe (İlgili Kavramlar Yakın Dosyada)

Bir metod, çağırdığı metodların yakınında bulunmalıdır. Çağıran metod üstte, çağrılan metod hemen altında yer alır.

```csharp
// DOGRU — cagiran ustunde, cagirilan hemen altinda
public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
{
    var customer = await GetCustomerOrThrowAsync(request.CustomerId, ct);
    var order = BuildOrder(customer, request);
    await SaveOrderAsync(order, ct);
    return order.Id;
}

private async Task<Customer> GetCustomerOrThrowAsync(Guid customerId, CancellationToken ct)
    => await _customerRepository.GetByIdAsync(customerId, ct)
        ?? throw new NotFoundException(nameof(Customer), customerId);

private static Order BuildOrder(Customer customer, CreateOrderCommand request)
    => Order.Create(customer.Id, request.ShippingAddress);

private async Task SaveOrderAsync(Order order, CancellationToken ct)
{
    await _orderRepository.AddAsync(order, ct);
    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 2. Yatay Biçimlendirme

### 2.1 Satır Uzunluğu

Satır uzunluğu **120 karakteri** geçmemelidir. Uzun zincirleme çağrılar alt satıra bölünür.

```csharp
// YANLIS — tek satirda cok uzun
var expensiveProducts = await _context.Products.Where(p => !p.IsDeleted && p.Price > 1000 && p.Stock > 0).OrderByDescending(p => p.Price).ToListAsync(ct);

// DOGRU — girintili zincir
var expensiveProducts = await _context.Products
    .Where(p => !p.IsDeleted && p.Price > 1000 && p.Stock > 0)
    .OrderByDescending(p => p.Price)
    .ToListAsync(ct);
```

### 2.2 Operatörler Etrafında Boşluk

Atama ve aritmetik operatörlerin iki yanına boşluk bırak. Metod parametrelerini ve köşeli parantezleri boşlukla ayırma.

```csharp
// YANLIS
var total=price*quantity+fee;
DoSomething( product , quantity );

// DOGRU
var total = price * quantity + fee;
DoSomething(product, quantity);
```

### 2.3 Girinti (Indentation)

Kapsam (scope) hiyerarşisini görsel olarak yansıtmak için her seviyede 4 boşluk girintileme kullan. Tab karakteri kullanma; `.editorconfig` ile `indent_style = space` zorunlu kılınır.

---

## 3. Dosya İçi Sıralama Kuralı

C# sınıf üyeleri aşağıdaki sıraya uyar:

```
1. static readonly alanlar / sabitler
2. private readonly instance alanları
3. Constructor(lar)
4. public metodlar (interface implementasyonları dahil)
5. private yardımcı metodlar
```

```csharp
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    // 1. Sabitler
    private const int MaxDescriptionLength = 500;

    // 2. Private alanlar
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    // 3. Constructor
    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    // 4. Public metodlar
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = BuildProduct(request);
        await SaveProductAsync(product, ct);
        return product.Id;
    }

    // 5. Private yardimci metodlar
    private static Product BuildProduct(CreateProductCommand request)
        => Product.Create(request.Name, request.Price, request.Stock);

    private async Task SaveProductAsync(Product product, CancellationToken ct)
    {
        await _productRepository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

---

## 4. Takım Kuralları

Biçimlendirme kuralları `.editorconfig` ile proje genelinde otomatik uygulanır. Bireysel tercih, takım standardının önüne geçemez.

```ini
# .editorconfig (proje koku)
[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8-bom
trim_trailing_whitespace = true
insert_final_newline = true
dotnet_sort_system_directives_first = true
```
