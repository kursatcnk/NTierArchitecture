# N-Tier Architecture

Ürün ve kategori yönetimi yapan bir ASP.NET Core 8 Web API üzerinden katmanlı mimariyi uyguladığım proje. Aynı alanı ikinci bir çözümde Clean Architecture ile yeniden kurmaya başladım; iki yaklaşımı yan yana karşılaştırmak için ikisi de bu depoda.

## Katmanlar

**N-Tier (çalışan sürüm)**

```
App.API            Controller'lar, tek tip cevap dönen CustomBaseController
App.Services       İş kuralları, DTO'lar, AutoMapper profilleri, FluentValidation
App.Repositories   EF Core DbContext, generic repository, Unit of Work, migration'lar
```

**Clean Architecture (devam ediyor)**

```
CleanApp.API       Sunum katmanı
App.Application    Use case'ler, repository sözleşmeleri, request/response modelleri
App.Domain         Entity'ler, ortak arayüzler
App.Persistence    Veri erişimi
```

## Neler var

- **Generic repository ve Unit of Work:** ortak CRUD tek yerde, kaydetme işlemi servis katmanından tek noktadan yapılıyor.
- **ServiceResult:** servisler veri, hata listesi ve HTTP durum kodunu tek nesnede döndürüyor; controller sadece bunu cevaba çeviriyor.
- **Doğrulama:** FluentValidation kuralları global bir filtreyle çalışıyor, hatalı istek controller'a ulaşmadan 400 dönüyor.
- **Hata yönetimi:** `IExceptionHandler` ile global ve kritik hata yakalayıcılar.
- **Audit:** `IAuditEntity` uygulayan kayıtlarda oluşturma / güncelleme tarihleri bir `SaveChangesInterceptor` ile otomatik doluyor.
- **NotFound filtresi:** id ile gelen isteklerde kayıt yoksa action hiç çalışmadan 404 dönüyor.
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

Gerekenler: .NET 8 SDK ve SQL Server.

```bash
git clone https://github.com/kursatcnk/NTierArchitecture.git
cd NTierArchitecture
dotnet ef database update --project Repositories --startup-project App.API
dotnet run --project App.API
```

Bağlantı dizesi `App.API/appsettings.Development.json` içindeki `ConnectionStrings:SqlServer` alanında; kendi sunucu adınla değiştir. Hazır veriyle denemek istersen `DbNLayerArchitecture.bak` yedeğini SQL Server'a geri yükleyebilirsin. Swagger arayüzü `/swagger` adresinde açılır.

## Lisans

[MIT](LICENSE)
