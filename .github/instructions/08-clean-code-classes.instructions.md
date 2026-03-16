---
name: Clean Code - Sınıflar
description: Robert C. Martin 'Clean Code' Chapter 10 — Classes. C# sınıf organizasyonu, SRP, cohesion ve değişime kapalı tasarım kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Sınıflar (Chapter 10)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 10.

---

## 1. Sınıf Organizasyonu (Üye Sıralama)

C# sınıf üyeleri aşağıdaki sırayla yazılır. Bu kural `04-clean-code-formatting.instructions.md` ile uyumludur.

```
1. static readonly sabitler
2. private readonly instance alanları
3. Constructor(lar) — az bağımlılıktan çoğa doğru
4. public özellikler (property)
5. public metodlar
6. private/protected yardımcı metodlar
```

---

## 2. Sınıflar Küçük Olmalı — Sorumluluk Ölçüsü

Sınıf satır sayısıyla değil, **sorumluluk sayısıyla** ölçülür. Sınıf adı birden fazla kelime gerektiriyorsa (`Manager`, `Processor`, `Super`, `Handler` birleşimleri) büyük ihtimalle birden fazla sorumluluğu var demektir.

```csharp
// YANLIS — tek sinif, çok sorumluluk
public class OrderService
{
    public Order CreateOrder(CreateOrderRequest request) { }      // is mantigi
    public void SendConfirmationEmail(Order order) { }           // bildirim
    public decimal CalculateTax(Order order) { }                 // vergi hesaplama
    public void LogOrderCreated(Order order) { }                 // loglama
    public void SaveOrder(Order order) { }                       // veri erisimi
}

// DOGRU — her sinif tek sorumluluk
internal sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    // Yalnizca siparis olusturma akisini orkestre eder
}

internal sealed class TaxCalculationService
{
    public decimal Calculate(Order order) { }
}

// Bildirim: ayri microservis veya domain event ile
// Loglama: pipeline behavior ile
// Veri erisimi: repository ile
```

---

## 3. Tek Sorumluluk Prensibi (SRP)

Bir sınıfın değişmesi için yalnızca bir nedeni olmalıdır.

```csharp
// YANLIS — iki degisim nedeni: (1) rapor formati, (2) rapor icerigi
public class OrderReport
{
    public string GenerateSummary(Order order) { }   // icerik degisirse degisir
    public byte[] ExportToPdf(string summary) { }    // format degisirse degisir
}

// DOGRU — her sinif tek degisim nedeni
public class OrderSummaryBuilder
{
    public string Build(Order order) { }
}

public class PdfExporter
{
    public byte[] Export(string content) { }
}
```

---

## 4. Cohesion — Uyum

Bir sınıfın metodları, sınıfın alan değişkenlerinin büyük çoğunluğunu kullanmalıdır. Az sayıda instance değişkenini kullanan birçok metod düşük uyuma (low cohesion) işaret eder; sınıf bölünmelidir.

```csharp
// YANLIS — dusuk uyum; metodlar birbirinden cok farkli alanlar kullaniyor
public class ProductManager
{
    private string _productName;
    private decimal _price;
    private string _reportTitle;
    private DateTime _reportDate;
    private string _exportPath;

    public void UpdatePrice(decimal price) { _price = price; }
    public string GenerateReport() { return $"{_reportTitle} - {_reportDate}"; }
    public void ExportToFile() { File.WriteAllText(_exportPath, "..."); }
}

// DOGRU — uyumlu, odaklanmis siniflar
public sealed class Product : BaseEntity<Guid>
{
    private string _name;
    private decimal _price;
    private int _stock;

    public void UpdatePrice(decimal price) { _price = price; }   // _price kullaniyor
    public void AddStock(int quantity) { _stock += quantity; }   // _stock kullaniyor
    public bool IsAvailable() => _stock > 0;                     // _stock kullaniyor
}
```

---

## 5. Değişim İçin Organize Et (OCP)

Yeni davranış eklemek için mevcut kodu değiştirmek yerine genişletmeyi tercih et. Bu CQRS deseninde doğal olarak elde edilir: yeni özellik = yeni Command/Query + Handler.

```csharp
// YANLIS — yeni odeme tipi her eklendiginde bu sinifi degistirmek gerekiyor
public class PaymentProcessor
{
    public void Process(Payment payment)
    {
        if (payment.Type == PaymentType.CreditCard) ProcessCreditCard(payment);
        else if (payment.Type == PaymentType.BankTransfer) ProcessBankTransfer(payment);
        else if (payment.Type == PaymentType.Crypto) ProcessCrypto(payment); // yeni eklendi
    }
}

// DOGRU — yeni odeme tipi = yeni sinif; mevcut kod degismez
public interface IPaymentStrategy
{
    bool CanHandle(PaymentType type);
    Task ProcessAsync(Payment payment, CancellationToken ct);
}

internal sealed class CreditCardPaymentStrategy : IPaymentStrategy { }
internal sealed class BankTransferPaymentStrategy : IPaymentStrategy { }
internal sealed class CryptoPaymentStrategy : IPaymentStrategy { }  // yeni eklendi; diger siniflar dokunulmadi
```

---

## 6. Değişimden İzole Et (DIP)

Sınıflar somut implementasyonlara değil, soyutlamalara bağımlı olmalıdır. Bu projedeki Repository + IUnitOfWork arayüzleri bu prensibin uygulamasıdır.

```csharp
// YANLIS — somut sinifa bagimlilik; test edemez, degistiremezsin
public sealed class CreateProductCommandHandler
{
    private readonly ProductServiceDbContext _context;  // somut EF DbContext

    public CreateProductCommandHandler(ProductServiceDbContext context)
    {
        _context = context;
    }
}

// DOGRU — soyutlamalara bagimlilik; mock'lanabilir, test edilebilir
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;  // soyutlama
    private readonly IUnitOfWork _unitOfWork;                // soyutlama

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
}
```

---

## 7. Küçük ve Çok Sayıda Sınıf

Az sayıda büyük sınıf yerine, çok sayıda küçük ve odaklı sınıf tercih edilir. Her sınıf küçük bir sözlük parçasıdır; bir bütün ansiklopedi değil.

```csharp
// YANLIS — her seyi bilen "God Class"
public sealed class ProductService
{
    public Task<Guid> CreateAsync(CreateProductCommand cmd, CancellationToken ct) { }
    public Task UpdateAsync(UpdateProductCommand cmd, CancellationToken ct) { }
    public Task DeleteAsync(Guid id, CancellationToken ct) { }
    public Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct) { }
    public Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct) { }
    public Task<IReadOnlyList<ProductDto>> SearchAsync(string query, CancellationToken ct) { }
    public Task<byte[]> ExportToCsvAsync(CancellationToken ct) { }
}

// DOGRU — her islem ayri handler; CQRS ile dogal SRP
internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid> { }
internal sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit> { }
internal sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit> { }
internal sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto> { }
internal sealed class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>> { }
```
