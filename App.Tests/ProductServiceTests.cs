using System.Net;
using App.Repositories;
using App.Services.Products;
using App.Services.Products.Create;
using App.Services.Products.UpdateStock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests
{
    public class ProductServiceTests : IAsyncLifetime
    {
        private readonly TestApi _api = new();

        public Task InitializeAsync() => _api.SeedAsync();
        public Task DisposeAsync() => _api.DisposeAsync().AsTask();

        [Fact]
        public async Task CreateAsync_adds_the_product_and_returns_its_url()
        {
            using var scope = _api.Services.CreateScope();
            var service = _api.Resolve<IProductService>(scope);
            var categoryId = await _api.Resolve<AppDbContext>(scope).Categories.Select(c => c.Id).FirstAsync();

            var result = await service.CreateAsync(new CreateProductRequest("Klavye", 900, 10, categoryId));

            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Created, result.Status);
            Assert.Equal($"api/product/{result.Data!.Id}", result.UrlAsCreated);
        }

        [Fact]
        public async Task CreateAsync_rejects_a_duplicate_name()
        {
            using var scope = _api.Services.CreateScope();
            var service = _api.Resolve<IProductService>(scope);
            var categoryId = await _api.Resolve<AppDbContext>(scope).Categories.Select(c => c.Id).FirstAsync();

            var result = await service.CreateAsync(new CreateProductRequest("Laptop", 100, 1, categoryId));

            Assert.True(result.IsFail);
            Assert.Contains("Ürün Zaten Bulunmaktadır", result.ErrorMessage!);
        }

        [Fact]
        public async Task GetByIdAsync_returns_not_found_for_a_missing_product()
        {
            using var scope = _api.Services.CreateScope();
            var result = await _api.Resolve<IProductService>(scope).GetByIdAsync(9999);

            Assert.True(result.IsFail);
            Assert.Equal(HttpStatusCode.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateStockAsync_changes_only_the_stock()
        {
            int id;
            string name;
            using (var scope = _api.Services.CreateScope())
            {
                var product = await _api.Resolve<AppDbContext>(scope).Products.AsNoTracking().FirstAsync();
                (id, name) = (product.Id, product.Name);
                var result = await _api.Resolve<IProductService>(scope).UpdateStockAsync(new UpdateProductStockRequest(id, 3));
                Assert.Equal(HttpStatusCode.NoContent, result.Status);
            }

            using (var scope = _api.Services.CreateScope())
            {
                var product = await _api.Resolve<AppDbContext>(scope).Products.AsNoTracking().SingleAsync(p => p.Id == id);
                Assert.Equal(3, product.Stock);
                Assert.Equal(name, product.Name);
                Assert.NotNull(product.Updated);
            }
        }

        [Fact]
        public async Task GetPagedAllListAsync_returns_the_requested_page()
        {
            using var scope = _api.Services.CreateScope();
            var service = _api.Resolve<IProductService>(scope);

            var all = (await service.GetAllListAsync()).Data!;
            var page = (await service.GetPagedAllListAsync(2, 3)).Data!;

            Assert.Equal(all.Skip(3).Take(3).Select(p => p.Id), page.Select(p => p.Id));
        }

        [Fact]
        public async Task Audit_interceptor_sets_the_created_date_on_insert()
        {
            using var scope = _api.Services.CreateScope();
            var products = await _api.Resolve<AppDbContext>(scope).Products.AsNoTracking().ToListAsync();

            Assert.NotEmpty(products);
            Assert.All(products, p => Assert.NotEqual(default, p.Crated));
            Assert.All(products, p => Assert.Null(p.Updated));
        }
    }
}
