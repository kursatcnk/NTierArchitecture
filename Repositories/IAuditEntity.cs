using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Repositories
{
    public interface IAuditEntity
    {

        public DateTime Crated { get; set; }
        public DateTime? Updated { get; set; }
    }
}
