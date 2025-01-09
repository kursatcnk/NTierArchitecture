using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Features.Products.Create
{
    public record CreateProductRequest(string Name, decimal Price, int Stock, int CategoryId);
}
