---
name: Clean Code - Birim Testler
description: Robert C. Martin 'Clean Code' Chapter 9 — Unit Tests. C# test kodunda F.I.R.S.T prensipleri, AAA yapısı ve temiz test yazım kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Birim Testler (Chapter 9)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 9.

---

## 1. TDD Üç Yasası

1. Başarısız bir birim testi olmadan üretim kodu yazma.
2. Derlemeyi bozacak kadar fazla başarısız test yazma; derlenemeyen kod başarısız testtir.
3. Mevcut başarısız testi geçirecekten fazla üretim kodu yazma.

---

## 2. Testleri Temiz Tut

Test kodu üretim kodu kadar önem taşır. Kirli testler, üretim kodunu değiştirmeyi olanaksız hale getirir çünkü her değişiklik testleri kırar ve düzeltme maliyeti yükselir.

```csharp
// YANLIS — okunaksiz, ne test ettigini anlamak zor
[Fact]
public async Task T1()
{
    var r = new ProductRepository(new DbContext());
    var p = new Product(); p.Name = "X"; p.Price = 10;
    r.Add(p);
    var result = r.Get(p.Id);
    Assert.NotNull(result);
}

// DOGRU — okunakli, niyet acik
[Fact]
public async Task GetByIdAsync_ExistingProduct_ReturnsCorrectProduct()
{
    // Arrange
    var product = Product.Create("Laptop", 15_000m, 10);
    await _repository.AddAsync(product, CancellationToken.None);
    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

    // Act
    var result = await _repository.GetByIdAsync(product.Id, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(product.Id, result.Id);
    Assert.Equal("Laptop", result.Name);
}
```

---

## 3. Test Başına Tek Kavram

Her test yalnızca bir davranışı doğrular. Birden fazla `Assert` bloğu olabilir; ancak hepsi aynı kavramı test etmelidir.

```csharp
// YANLIS — tek test, birden fazla kavram
[Fact]
public void Product_Tests()
{
    var product = Product.Create("Laptop", 15_000m, 10);
    Assert.Equal("Laptop", product.Name);             // olusturma
    product.UpdatePrice(20_000m);
    Assert.Equal(20_000m, product.Price);             // fiyat guncelleme
    product.DecreaseStock(3);
    Assert.Equal(7, product.Stock);                   // stok azaltma
}

// DOGRU — her davranis ayri test
[Fact]
public void Create_ValidParameters_SetsNameCorrectly()
{
    var product = Product.Create("Laptop", 15_000m, 10);
    Assert.Equal("Laptop", product.Name);
}

[Fact]
public void UpdatePrice_ValidPrice_UpdatesPriceCorrectly()
{
    var product = Product.Create("Laptop", 15_000m, 10);
    product.UpdatePrice(20_000m);
    Assert.Equal(20_000m, product.Price);
}

[Fact]
public void DecreaseStock_ValidQuantity_DecreasesStockByQuantity()
{
    var product = Product.Create("Laptop", 15_000m, 10);
    product.DecreaseStock(3);
    Assert.Equal(7, product.Stock);
}
```

---

## 4. F.I.R.S.T Prensipleri

| Harf | Prensip | Açıklama |
|---|---|---|
| **F** | Fast | Testler hızlı çalışır; yüzlerce test saniyeler içinde biter. |
| **I** | Independent | Testler birbirinden bağımsızdır; sıraları önemli değildir. |
| **R** | Repeatable | Testler her ortamda (CI, dev, prod benzeri) aynı sonucu verir. |
| **S** | Self-Validating | Test kendi başarısını/başarısızlığını bildiren boolean sonuç döndürür; log okumak gerekmez. |
| **T** | Timely | Testler üretim kodundan önce yazılır (TDD). |

---

### 4.1 Fast — Hızlı

Veritabanı, ağ ve diskten bağımsız unit test yaz. Entegrasyon testleri için ayrı proje oluştur.

```csharp
// YANLIS — gercek veritabani kullanan yavas test
[Fact]
public async Task Handle_ValidCommand_PersistsProduct()
{
    // Gercek PostgreSQL baglantisi — yavas, CI ortaminda calismaayabilir
    var context = new ProductServiceDbContext(realConnectionOptions);
    var handler = new CreateProductCommandHandler(new ProductRepository(context), context);
    // ...
}

// DOGRU — in-memory / mock ile izole, hizli test
[Fact]
public async Task Handle_ValidCommand_ReturnsNewProductId()
{
    // Arrange
    var repository = Substitute.For<IProductRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new CreateProductCommandHandler(repository, unitOfWork);
    var command = new CreateProductCommand("Laptop", 15_000m, 10);

    // Act
    var id = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotEqual(Guid.Empty, id);
    await repository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

---

### 4.2 Independent — Bağımsız

Her test kendi bağlamını `Arrange` aşamasında kurar. Paylaşılan state'e güvenmez.

```csharp
// YANLIS — testler arasi paylasilan state
private static Product _sharedProduct = Product.Create("Test", 100m, 5);

[Fact]
public void Test1_DecreasesStock() { _sharedProduct.DecreaseStock(2); }

[Fact]
public void Test2_ChecksStock() { Assert.Equal(5, _sharedProduct.Stock); } // Test1'e bagimli!

// DOGRU — her test kendi product'ini olusturuyor
[Fact]
public void DecreaseStock_ByTwo_ReducesStockFromFive()
{
    var product = Product.Create("Test", 100m, 5);
    product.DecreaseStock(2);
    Assert.Equal(3, product.Stock);
}
```

---

### 4.3 Repeatable — Tekrarlanabilir

Rastgele değer, `DateTime.Now`, sistem saati gibi dış bağımlılıklar soyutlanır.

```csharp
// YANLIS — DateTime.Now'a bagimli; CI'da farkli sonuc verebilir
[Fact]
public void IsExpired_TodayIsAfterExpiry_ReturnsTrue()
{
    var subscription = Subscription.Create(expiresAt: DateTime.Now.AddDays(-1));
    Assert.True(subscription.IsExpired());
}

// DOGRU — zaman soyutlanmis; deterministik
[Fact]
public void IsExpired_CurrentDateIsAfterExpiry_ReturnsTrue()
{
    var fixedNow = new DateTime(2026, 3, 16);
    var subscription = Subscription.Create(expiresAt: new DateTime(2026, 3, 15));
    Assert.True(subscription.IsExpired(fixedNow));
}
```

---

## 5. Test İsimlendirme Kuralı

`{TestedMethod}_{Scenario}_{ExpectedResult}` formatı kullanılır.

```csharp
// YANLIS
[Fact] public void Test1() { }
[Fact] public void ProductTest() { }

// DOGRU
[Fact] public void Create_NegativePrice_ThrowsArgumentOutOfRangeException() { }
[Fact] public void GetNetPrice_WithTenPercentDiscount_ReturnsNinetyPercentOfPrice() { }
[Fact] public void Handle_ProductNotFound_ThrowsNotFoundException() { }
```

---

## 6. Arrange-Act-Assert (AAA) Yapısı

Test gövdesi üç net bölüme ayrılır; bölümler yorumla işaretlenmez, boş satırla ayrılır.

```csharp
[Fact]
public async Task Handle_ValidCommand_CreatesProductWithCorrectName()
{
    var repository = Substitute.For<IProductRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new CreateProductCommandHandler(repository, unitOfWork);
    var command = new CreateProductCommand("Gaming Laptop", 25_000m, 5);

    var id = await handler.Handle(command, CancellationToken.None);

    Assert.NotEqual(Guid.Empty, id);
}
```
