---
name: Mimari Talimat ve Kuralları
description: Projede uygulanacak herhangi bir mimarisel yapıda uyulması gereken zorunlu kurallar listesi.
applyTo: "**/*.cs"
---

# Clean Architecture Kuralları

Bu talimat dosyası projedeki tüm servislerde uygulanacak mimari kararları ve zorunlu kalıpları tanımlar.
Seçilen desenler: **CQRS + MediatR**, **Repository + Unit of Work**, **Exception tabanlı hata yönetimi**.

---

## 1. Katman Bağımlılık Kuralları (Zorunlu)

```
Domain ← Application ← Infrastructure
                  ↑
                 API
```

| Katman | Bağımlı Olabileceği Katmanlar | Yasak Bağımlılıklar |
|---|---|---|
| `Domain` | Hiçbir proje | Application, Infrastructure, API |
| `Application` | `Domain` | Infrastructure, API |
| `Infrastructure` | `Domain`, `Application` | API |
| `API` | `Application`, `Infrastructure` (yalnızca DI kaydı için) | Domain'e doğrudan erişim |

**Kural:** `API` katmanı iş mantığı içermez. `Infrastructure` yalnızca `API/Program.cs` içindeki DI kayıt extension'ları aracılığıyla referans alınır.

---

## 2. Klasör Yapısı (Zorunlu)

```
src/{ServiceName}/
  Domain/
    Abstractions/       ← BaseEntity, IRepository, IUnitOfWork, Domain exceptions
    Entities/           ← Entity sınıfları
    Enums/              ← Domain enum'ları
  Application/
    Features/
      {FeatureName}/
        Commands/
          {Action}Command.cs
          {Action}CommandHandler.cs
          {Action}CommandValidator.cs   (opsiyonel)
        Queries/
          {Action}Query.cs
          {Action}QueryHandler.cs
        DTOs/
          {Name}Dto.cs
    Behaviors/          ← Pipeline behavior'ları
    Abstractions/       ← IAplicationDbContext (opsiyonel), uygulama-seviye arayüzler
  Infrastructure/
    Persistence/
      {ServiceName}DbContext.cs
      Configurations/   ← IEntityTypeConfiguration<T> implementasyonları
      Repositories/     ← IRepository<T> implementasyonları
    Extensions/
      InfrastructureServiceExtensions.cs
  API/
    Controllers/
    Extensions/
      ApiServiceExtensions.cs
```

---

## 3. Domain Katmanı

### 3.1 Entity Kuralları

Entity yazımı için **generate-entity** yeteneği kuralları geçerlidir ve zorunludur. Özet:

- Tüm entity'ler `BaseEntity<Guid>`'den türer.
- Constructor'lar `private`; nesne yalnızca `static Create(...)` factory method ile oluşturulur.
- Property setter'lar `private set`; encapsulation korunur.
- Koleksiyonlar `IReadOnlyList<T>` olarak expose edilir.
- Domain doğrulaması BCL guard yöntemleriyle yapılır (`ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentOutOfRangeException.ThrowIfNegativeOrZero`).
- Entity'ler `sealed`; hiyerarşi gerekiyorsa `abstract base + sealed leaf` kalıbı kullanılır.

### 3.2 Repository Arayüzleri

`IRepository<T>` ve `IUnitOfWork` **Domain/Abstractions/** altında tanımlanır.

```csharp
// Domain/Abstractions/IRepository.cs
public interface IRepository<T> where T : BaseEntity<Guid>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

// Domain/Abstractions/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Kural:** Servis-spesifik repository arayüzleri de `Domain/Abstractions/` altında tanımlanır (örn. `IProductRepository : IRepository<Product>`).

### 3.3 Domain Exception'ları

```csharp
// Domain/Abstractions/DomainException.cs
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

// Domain/Abstractions/NotFoundException.cs
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.") { }
}

// Domain/Abstractions/BusinessRuleViolationException.cs
public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
```

**Kural:** Entity içindeki iş kuralı ihlalleri `BusinessRuleViolationException` veya BCL exception'ları (`InvalidOperationException`) fırlatır. `DomainException` doğrudan fırlatılmaz; türetilmiş sınıflar kullanılır.

---

## 4. Application Katmanı (CQRS + MediatR)

### 4.1 Command / Query Tanımı

```csharp
// Commands/CreateProduct/CreateProductCommand.cs
public sealed record CreateProductCommand(string Name, decimal Price, int Stock) : IRequest<Guid>;

// Queries/GetProductById/GetProductByIdQuery.cs
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
```

**Kurallar:**
- Command ve Query'ler `sealed record` olarak tanımlanır.
- Command sonucu genellikle oluşturulan entity'nin `Id`'si (`Guid`) veya `Unit`'tir.
- Query sonucu her zaman bir DTO'dur; Entity doğrudan döndürülmez.

### 4.2 Handler Tanımı

```csharp
// Commands/CreateProduct/CreateProductCommandHandler.cs
internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price, request.Stock);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
```

**Kurallar:**
- Handler'lar `internal sealed class` olarak tanımlanır.
- Handler içinde doğrudan `DbContext` kullanılmaz; repository ve `IUnitOfWork` kullanılır.
- Her handler tek bir `IRequest` tipini işler (Single Responsibility).

### 4.3 Validator (Opsiyonel, FluentValidation)

```csharp
// Commands/CreateProduct/CreateProductCommandValidator.cs
internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
```

### 4.4 Pipeline Behavior

Validation pipeline behavior `Application/Behaviors/` altında tanımlanır ve `ValidationException` fırlatır.

```csharp
// Application/Behaviors/ValidationBehavior.cs
internal sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // IValidator<TRequest> koleksiyonuyla çalışır
}
```

### 4.5 DTO Kuralları

```csharp
// DTOs/ProductDto.cs
public sealed record ProductDto(Guid Id, string Name, decimal Price, int Stock);
```

- DTO'lar `sealed record` olarak tanımlanır.
- Entity → DTO dönüşümü handler içinde manuel yapılır veya mapping extension method ile yapılır.
- AutoMapper **kullanılmaz**; explicit mapping tercih edilir.

---

## 5. Infrastructure Katmanı

### 5.1 DbContext

```csharp
// Infrastructure/Persistence/{ServiceName}DbContext.cs
public sealed class ProductServiceDbContext : DbContext, IUnitOfWork
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductServiceDbContext).Assembly);
    }
}
```

**Kural:** `DbContext`, `IUnitOfWork` arayüzünü implemente eder. Bu sayede Application katmanı `SaveChangesAsync`'i `IUnitOfWork` üzerinden çağırır.

### 5.2 Repository Implementasyonu

```csharp
// Infrastructure/Persistence/Repositories/ProductRepository.cs
internal sealed class ProductRepository : IProductRepository
{
    private readonly ProductServiceDbContext _context;

    public ProductRepository(ProductServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

    // Diğer metodlar...
}
```

**Kural:** Repository implementasyonları `internal sealed class`'tır; dışarıya sadece arayüz üzerinden erişilir.

### 5.3 Entity Configuration

```csharp
// Infrastructure/Persistence/Configurations/ProductConfiguration.cs
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.HasQueryFilter(p => !p.IsDeleted); // Global soft-delete filter
    }
}
```

### 5.4 DI Kayıt Extension

```csharp
// Infrastructure/Extensions/InfrastructureServiceExtensions.cs
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProductServiceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ProductServiceDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
```

---

## 6. API Katmanı

### 6.1 Controller Yapısı

```csharp
// API/Controllers/ProductsController.cs
[ApiController]
[Route("api/v1/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductByIdQuery(id), ct);
        return Ok(result);
    }
}
```

**Kurallar:**
- Controller'lar yalnızca `ISender` (MediatR) bağımlılığı alır.
- İş mantığı, doğrulama ve veri erişimi controller içinde bulunmaz.
- Route şablonu: `api/v1/[controller]` (versiyonlanmış).
- Controller sınıfları `sealed`'dir.

### 6.2 Global Exception Middleware

```csharp
// API/Middleware/ExceptionHandlingMiddleware.cs
internal sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (BusinessRuleViolationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}
```

**Kural:** Exception'lar controller içinde catch edilmez; global middleware tarafından ele alınır.

---

## 7. Genel Adlandırma Kuralları

| Yapı | Kural | Örnek |
|---|---|---|
| Command | `{Eylem}{Nesne}Command` | `CreateProductCommand` |
| Query | `{Eylem}{Nesne}Query` | `GetProductByIdQuery` |
| Handler | `{Command/Query adı}Handler` | `CreateProductCommandHandler` |
| Repository arayüzü | `I{Nesne}Repository` | `IProductRepository` |
| DTO | `{Nesne}Dto` | `ProductDto` |
| DbContext | `{ServiceName}DbContext` | `ProductServiceDbContext` |
| Extension method sınıfı | `{Katman}ServiceExtensions` | `InfrastructureServiceExtensions` |
| Exception | `{Sebep}Exception` | `NotFoundException` |

---

## 8. Kesinlikle Yasak Olanlar

- Domain entity'lerinin constructor'larının `public` tanımlanması.
- Application katmanından `DbContext`'e doğrudan erişim (yalnızca `IRepository` veya `IUnitOfWork` kullanılır).
- Controller içinde iş mantığı veya veri erişim kodu bulunması.
- AutoMapper kullanımı.
- Entity'lerin DTO olarak doğrudan döndürülmesi.
- `virtual` navigation property kullanımı (lazy loading engellenir).
- Handler'ların `public` veya non-`sealed` tanımlanması.

