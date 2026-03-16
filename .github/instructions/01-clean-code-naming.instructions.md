---
name: Clean Code - Anlamlı İsimler
description: Robert C. Martin 'Clean Code' Chapter 2 — Meaningful Names. C# kodunda değişken, metod, sınıf ve namespace adlandırma kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Anlamlı İsimler (Chapter 2)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 2.

---

## 1. Niyeti Açıklayan İsimler Kullan

İsim; neden var olduğunu, ne yaptığını ve nasıl kullanıldığını açıklamalıdır. Yorum gerektiren bir isim başarısız bir isimdir.

```csharp
// YANLIS
int d; // elapsed time in days

// DOGRU
int elapsedTimeInDays;
int daysSinceCreation;
```

---

## 2. Dezenformasyondan Kaçın

Okuyucuyu yanıltacak kısaltmalar veya yanlış bağlam taşıyan isimler kullanma.

```csharp
// YANLIS — "List" kelimesi özel anlam taşır; koleksiyon değilse kullanma
var productList = GetActiveProducts(); // aslında IEnumerable<Product>

// DOGRU
var activeProducts = GetActiveProducts();

// YANLIS — birbirine çok benzeyen isimler
ProductServiceForExistingAccounts
ProductServiceForNewAccounts

// DOGRU
ExistingAccountProductService
NewAccountProductService
```

---

## 3. Anlamlı Farklılıklar Yap

Sayı serisi (`a1`, `a2`) ve gürültü kelimeleri (`Info`, `Data`, `The`) anlam taşımaz.

```csharp
// YANLIS
public Product GetProduct(Product p1, Product p2) { }
public ProductData GetProductData() { }  // ProductInfo ile farkı ne?

// DOGRU
public Product MergeProducts(Product source, Product target) { }
public Product GetProductById(Guid id) { }
```

---

## 4. Telaffuz Edilebilir İsimler Kullan

```csharp
// YANLIS
private DateTime genymdhms;
private int cntStkRcds;

// DOGRU
private DateTime generationTimestamp;
private int stockRecordCount;
```

---

## 5. Aranabilir İsimler Kullan

Tek harfli isimler ve sayısal sabitler kod içinde aranması güç sihirli değerler yaratır.

```csharp
// YANLIS
for (int i = 0; i < 34; i++)
{
    tasks[i].dailyRate *= 4;
}

// DOGRU
const int WorkDaysPerWeek = 5;
const int NumberOfTasks = taskCount;

for (int taskIndex = 0; taskIndex < NumberOfTasks; taskIndex++)
{
    tasks[taskIndex].DailyRate *= WorkDaysPerWeek;
}
```

**Kural:** Tek harfli değişkenler yalnızca kısa yerel `for` döngülerinde kabul edilir.

---

## 6. Kodlama / Prefiks Kullanma

Hungarian notation ve üye prefiksleri (`m_`, `_`, `I` interface prefiksi hariç) yasaktır.

```csharp
// YANLIS
string m_productName;
string strProductName;
IProductRepository iProductRepository;

// DOGRU
string productName;
IProductRepository productRepository;
```

**İstisna:** C# arayüzleri `I` prefixi alır (`IRepository`, `IUnitOfWork`). Bu BCL ve .NET konvansiyonunun zorunlu parçasıdır.

---

## 7. Sınıf İsimleri: İsim, Metod İsimleri: Fiil

```csharp
// YANLIS — sınıf ismi fiil
public class ProcessOrder { }
public class Manager { }  // belirsiz

// DOGRU — sınıf ismi somut isim
public class OrderProcessor { }
public class ProductRepository { }

// YANLIS — metod ismi isim
public Product Product() { }

// DOGRU — metod ismi fiil/fiil öbeği
public Product GetProductById(Guid id) { }
public void UpdateStock(int quantity) { }
public bool IsStockAvailable(int requestedQuantity) { }
```

---

## 8. Kavram Başına Tek Kelime

Birbirinin yerine kullanılan `Fetch`, `Retrieve`, `Get` gibi kelimeler karışıklık yaratır.

```csharp
// YANLIS — aynı kavram için farklı kelimeler
ProductRepository.FetchById()
OrderRepository.RetrieveById()
CustomerRepository.GetById()

// DOGRU — tutarlı sözlük
ProductRepository.GetByIdAsync()
OrderRepository.GetByIdAsync()
CustomerRepository.GetByIdAsync()
```

---

## 9. Çözüm ve Problem Alanı İsimleri

Teknik kavramlar için çözüm alanı isimlerini (`Repository`, `Factory`, `Command`, `Handler`) tercih et. Domain kavramları için problem alanı isimlerini kullan.

```csharp
// Cozum alani — teknik desen isimleri dogrudur
CreateProductCommandHandler
ProductRepository
ValidationBehavior

// Problem alani — domain kavramlari
OrderFulfillment
InventoryAllocation
PriceCalculation
```

---

## 10. Anlamlı Bağlam Ekle, Gereksiz Bağlam Ekleme

```csharp
// YANLIS — sinif adi zaten baglam sagliyor; tekrar etme
public class Product
{
    public string ProductName { get; private set; }  // "Product" tekrari
    public decimal ProductPrice { get; private set; }
}

// DOGRU
public class Product
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
}

// YANLIS — her seye "PS" prefiksi eklemek (ProductService kisaltmasi)
public class PSProduct { }
public class PSOrder { }

// DOGRU
public class Product { }
public class Order { }
```
