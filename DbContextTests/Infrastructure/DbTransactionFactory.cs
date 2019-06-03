using DbContextTests.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    class DbTransactionFactory : ITransactionFactory
    {
        private readonly DbContext db;

        class DbTrasaction : ITransaction
        {
            private readonly DbContextTransaction dbContextTransaction;

            public DbTrasaction(DbContextTransaction dbContextTransaction)
            {
                this.dbContextTransaction = dbContextTransaction;
            }

            public void Commit() => dbContextTransaction.Commit();

            public void Dispose() => dbContextTransaction.Dispose();
        }

        public DbTransactionFactory(DbContext db)
        {
            this.db = db;
        }

        public ITransaction GetTransaction() => new DbTrasaction(db.Database.BeginTransaction());
    }
}
