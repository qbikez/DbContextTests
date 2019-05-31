using OtherContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    public class OtherContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }
    }
}
