using App.Repositories.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Repositories.Products
{
    public class Product : BaseEntity<int>, IAuditEntity
    {

        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public DateTime Crated { get; set; }
        public DateTime? Updated { get; set; }

    }
}
