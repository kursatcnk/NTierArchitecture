using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Products.Create
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün İsmi Boş Geçilemez")
            .Length(3, 10).WithMessage("Ürün İsmi 3 İle 10 Karakter Arasında Olmalıdır.");

            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Ürün Fiyatı 0 Dan Büyük Olmalıdır");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Ürünün Kategori Değeri 0 Dan Büyük olmalıdır");

            RuleFor(x => x.Stock).InclusiveBetween(1, 100).WithMessage("Stok Adedi 1 İle 100 Arasında Olmalıdır");
        }
    }
}
