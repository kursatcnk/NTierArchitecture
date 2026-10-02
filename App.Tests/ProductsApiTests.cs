using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using App.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests
{
    public class ProductsApiTests : IAsyncLifetime
    {
        private readonly TestApi _api = new();
        private HttpClient _client = default!;

        public async Task InitializeAsync()
        {
            await _api.SeedAsync();
            _client = _api.CreateClient();
        }

        public Task DisposeAsync() => _api.DisposeAsync().AsTask();

        private async Task<int> FirstCategoryIdAsync()
        {
            using var scope = _api.Services.CreateScope();
            return await _api.Resolve<AppDbContext>(scope).Categories.Select(c => c.Id).FirstAsync();
        }

        [Fact]
        public async Task Get_all_returns_the_seeded_products()
        {
            var response = await _client.GetAsync("/api/products");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(7, body.RootElement.GetProperty("data").GetArrayLength());
        }

        [Fact]
        public async Task Post_with_invalid_body_returns_validation_errors()
        {
            var response = await _client.PostAsJsonAsync("/api/products", new { name = "A", price = 0, stock = 0, categoryId = 0 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var errors = body.RootElement.GetProperty("errorMessage").EnumerateArray().Select(e => e.GetString()).ToList();
            Assert.Contains("Ürün İsmi 3 İle 10 Karakter Arasında Olmalıdır.", errors);
            Assert.Contains("Ürün Fiyatı 0 Dan Büyük Olmalıdır", errors);
        }

        [Fact]
        public async Task Post_with_valid_body_returns_created()
        {
            var response = await _client.PostAsJsonAsync("/api/products",
                new { name = "Mouse", price = 450, stock = 20, categoryId = await FirstCategoryIdAsync() });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task Get_by_missing_id_returns_not_found()
        {
            var response = await _client.GetAsync("/api/products/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_missing_id_is_stopped_by_not_found_filter()
        {
            var response = await _client.DeleteAsync("/api/products/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_existing_product_returns_no_content()
        {
            var all = await _client.GetFromJsonAsync<JsonElement>("/api/products");
            var id = all.GetProperty("data")[0].GetProperty("id").GetInt32();

            var response = await _client.DeleteAsync($"/api/products/{id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/products/{id}")).StatusCode);
        }
    }
}
