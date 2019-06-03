using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    public interface IContextFactory<TContext>
        where TContext : DbContext
    {
        TContext Create();
    }
}
