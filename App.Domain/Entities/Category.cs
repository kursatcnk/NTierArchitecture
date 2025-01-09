using App.Domain.Entities.Common;
using App.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.Categories
{
    public class Category : BaseEntity<int>, IAuditEntity
    {
        public string Name { get; set; } = default!;

        public List<Product>? Products { get; set; }
        public DateTime Crated { get; set; }
        public DateTime? Updated { get; set; }
    }
}
