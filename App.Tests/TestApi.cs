using App.Repositories;
using App.Repositories.Interceptors;
using App.Repositories.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests
{
    // API'yi gerçek pipeline'ıyla (filtreler, validator'lar, exception handler'lar) ayağa kaldırıyor,
    // sadece SQL Server yerine her örnek için ayrı bir InMemory veritabanı kullanıyor.
    public class TestApi : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Development değil; Program.cs'teki migrate + seed InMemory'de çalışmasın.
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var options = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(options);

                services.AddDbContext<AppDbContext>(o => o
                    .UseInMemoryDatabase(_databaseName)
                    .AddInterceptors(new AuditDbContextInterceptor()));
            });
        }

        public async Task SeedAsync()
        {
            using var scope = Services.CreateScope();
            await DevelopmentSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        public T Resolve<T>(IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
    }
}
