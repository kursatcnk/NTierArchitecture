using App.Repositories.Categories;
using App.Repositories.Products;
using Microsoft.EntityFrameworkCore;

namespace App.Repositories.Seed
{
    // Development ortamında boş veritabanına örnek kategori ve ürün ekliyor.
    // Tabloda kayıt varsa hiçbir şeye dokunmuyor.
    public static class DevelopmentSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Categories.AnyAsync()) return;

            // Ürün isimleri validator'daki 3-10 karakter kuralına uyuyor, güncelleme denerken takılmasın.
            var categories = new List<Category>
            {
                new()
                {
                    Name = "Elektronik",
                    Products =
                    [
                        new() { Name = "Laptop", Price = 32500, Stock = 12 },
                        new() { Name = "Kulaklık", Price = 1850, Stock = 40 },
                        new() { Name = "Monitör", Price = 7400, Stock = 18 }
                    ]
                },
                new()
                {
                    Name = "Kitap",
                    Products =
                    [
                        new() { Name = "Roman", Price = 210, Stock = 75 },
                        new() { Name = "Sözlük", Price = 340, Stock = 25 }
                    ]
                },
                new()
                {
                    Name = "Spor",
                    Products =
                    [
                        new() { Name = "Dambıl", Price = 950, Stock = 30 },
                        new() { Name = "Mat", Price = 420, Stock = 55 }
                    ]
                }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
