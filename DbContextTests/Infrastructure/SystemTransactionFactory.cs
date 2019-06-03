using DbContextTests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    class SystemTransactionFactory : ITransactionFactory
    {
        class SystemTransaction : ITransaction
        {
            private readonly System.Transactions.TransactionScope transactionScope;

            public SystemTransaction(System.Transactions.TransactionScope transactionScope)
            {
                this.transactionScope = transactionScope;
            }
            public void Commit() => transactionScope.Complete();

            public void Dispose() => transactionScope.Dispose();
        }
        public ITransaction GetTransaction() => new SystemTransaction(new System.Transactions.TransactionScope());
    }
}
