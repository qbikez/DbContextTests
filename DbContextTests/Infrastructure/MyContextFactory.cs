using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    public class MyContextFactory : IContextFactory<MyContext>
    {
        public MyContext Create() => new MyContext();
    }
}
