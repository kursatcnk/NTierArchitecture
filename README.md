# N-Tier Architecture

Ürün ve kategori yönetimi yapan bir ASP.NET Core 8 Web API üzerinden katmanlı mimariyi uyguladığım proje. Çalışan sürüm N-Tier. Aynı alanı Clean Architecture ile yeniden kurmaya da başladım, o kısım henüz tamamlanmadı (aşağıda ayrıca anlattım).

## Katmanlar

```
App.API            Controller'lar, tek tip cevap dönen CustomBaseController
App.Services       İş kuralları, DTO'lar, AutoMapper profilleri, FluentValidation
App.Repositories   EF Core DbContext, generic repository, Unit of Work, migration'lar, development seed
App.Tests          xUnit: servis kuralları ve API davranışı
```

## Neler var

- **Generic repository ve Unit of Work:** ortak CRUD tek yerde, kaydetme işlemi servis katmanından tek noktadan yapılıyor.
- **ServiceResult:** servisler veri, hata listesi ve HTTP durum kodunu tek nesnede döndürüyor; controller sadece bunu cevaba çeviriyor.
- **Doğrulama:** FluentValidation kuralları global bir filtreyle çalışıyor, hatalı istek controller'a ulaşmadan 400 dönüyor.
- **Hata yönetimi:** `IExceptionHandler` ile global ve kritik hata yakalayıcılar.
- **Audit:** `IAuditEntity` uygulayan kayıtlarda oluşturma / güncelleme tarihleri bir `SaveChangesInterceptor` ile otomatik doluyor.
- **NotFound filtresi:** id ile gelen güncelleme ve silme isteklerinde kayıt yoksa action hiç çalışmadan 404 dönüyor.
- **Sayfalama:** `GET /api/products/{pageNumber}/{pageSize}`.

## Endpoint'ler

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/api/products` | Tüm ürünler |
| GET | `/api/products/{page}/{size}` | Sayfalı ürün listesi |
| GET | `/api/products/{id}` | Tek ürün |
| POST | `/api/products` | Ürün ekle |
| PUT | `/api/products/{id}` | Ürün güncelle |
| PATCH | `/api/products/stock` | Stok güncelle |
| DELETE | `/api/products/{id}` | Ürün sil |
| GET | `/api/categories` | Tüm kategoriler |
| GET | `/api/categories/products` | Kategoriler ve ürünleri |
| GET | `/api/categories/{id}/products` | Bir kategori ve ürünleri |
| POST / PUT / DELETE | `/api/categories/...` | Kategori ekle / güncelle / sil |

## Çalıştırma

Gerekenler: .NET 8 SDK ve SQL Server (LocalDB yeterli).

```bash
git clone https://github.com/kursatcnk/NTierArchitecture.git
cd NTierArchitecture
dotnet run --project App.API
```

Development ortamında uygulama açılırken bekleyen migration'ları uygular ve veritabanı boşsa örnek veri ekler (3 kategori, 7 ürün). Var olan veriye dokunmaz. Swagger arayüzü `/swagger` adresinde açılır.

Bağlantı dizesi `App.API/appsettings.Development.json` içindeki `ConnectionStrings:SqlServer` alanında. Dosyayı değiştirmeden başka bir sunucu kullanmak için:

```bash
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=(localdb)\MSSQLLocalDB;Database=DbNLayerArchitecture;Trusted_Connection=True;TrustServerCertificate=True" --project App.API
```

Migration'ı elle uygulamak istersen:

```bash
dotnet ef database update --project Repositories --startup-project App.API
```

## Testler

```bash
dotnet test
```

Testler API'yi gerçek pipeline'ıyla (filtreler, validator'lar, exception handler'lar) EF Core InMemory veritabanı üzerinde çalıştırır; SQL Server gerekmez. Ürün ekleme, isim tekrarı, stok güncelleme, sayfalama, audit alanları, validasyon hataları ve 404 davranışlarını kapsar.

## Clean Architecture (work in progress)

`CleanApp.API`, `App.Application`, `App.Domain` ve `App.Persistence` projeleri aynı alanın Clean Architecture sürümü için başlattığım iskelet. Şu an:

- `App.Domain`: `Product`, `Category`, ortak entity arayüzleri hazır.
- `App.Application`: repository sözleşmeleri ve ürün oluşturma / güncelleme request'leri ile validator'ları var; servisler henüz yok.
- `App.Persistence`: boş, veri erişimi taşınmadı.
- `CleanApp.API`: şablondaki örnek controller'dan ibaret, ürün ve kategori uçları yok.

Projeler solution içinde build oluyor ama çalışan bir API değil. Çalışan sürüm yukarıdaki N-Tier.

## Lisans

[MIT](LICENSE)
