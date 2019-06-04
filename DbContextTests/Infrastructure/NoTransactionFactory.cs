using DbContextTests.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    class NoTransactionFactory : ITransactionFactory
    {
        class NoTransaction : ITransaction
        {
            public NoTransaction()
            {
            }

            public void Commit() { }

            public void Dispose() { }
        }

        public NoTransactionFactory()
        {
        }

        public ITransaction GetTransaction() => new NoTransaction();
    }
}
