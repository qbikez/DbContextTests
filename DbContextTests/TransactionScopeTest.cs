using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OtherContext;

namespace DbContextTests
{
    [TestClass]
    public class TransactionScopeTest
    {
        private int perfLoops = 1000;
        private int userId = 1;
        private string perfLogFile = "perf.csv";

        [TestMethod]
        public void rollback_transaction_in_multiple_same_contexts()
        {
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            using (var tran = new TransactionScope())
            {
                using (var db = new MyContext())
                {
                    IncreateUserOrdersCount(db);
                }

                using (var db = new MyContext())
                {
                    AddOrder(db);
                }
            }

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);

                Assert.AreEqual(initialCount, user.OrdersCount);
                Assert.AreEqual(initialCount, user.Orders.Count());
            }
        }

        [TestMethod]
        public void rollback_transaction_in_multiple_same_contexts_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);
                    }

                    using (var db = new MyContext())
                    {
                        AddOrder(db);
                    }
                }
            });
        }

        [TestMethod]
        public void commit_transaction_in_multiple_same_contexts_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);
                    }

                    using (var db = new MyContext())
                    {
                        AddOrder(db);
                    }

                    tran.Complete();
                }
            });
        }

        [TestMethod]
        public void rollback_transaction_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);

                        AddOrder(db);
                    }
                }
            });
        }

        [TestMethod]
        public void rollback_transaction_in_single_context()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            using (var tran = new TransactionScope())
            {
                using (var db = new MyContext())
                {
                    IncreateUserOrdersCount(db);

                    AddOrder(db);
                }
            }

        }

        [TestMethod]
        public void no_transactions_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);

                        AddOrder(db);
                    }
                }
            });
        }

        [TestMethod]
        public void commit_db_transactions_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            MeasurePerf(() =>
            {
                using (var db = new MyContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        IncreateUserOrdersCount(db);

                        AddOrder(db);
                        tran.Commit();
                    }
                }

            });
        }

        [TestMethod]
        public void rollback_db_transactions_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            MeasurePerf(() =>
            {
                using (var db = new MyContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        IncreateUserOrdersCount(db);

                        AddOrder(db);
                        tran.Rollback();
                    }
                }

            });
        }


        [TestMethod]
        public void commit_transaction_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);

                        AddOrder(db);
                    }
                    tran.Complete();
                }
            });
        }

        [TestMethod]
        public void rollback_transaction_in_multiple_different_contexts()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            using (var tran = new TransactionScope())
            {
                using (var db = new MyContext())
                {
                    IncreateUserOrdersCount(db);
                }

                using (var db = new OtherContext())
                {
                    AddInvoice(db);
                }
            }

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);

                Assert.AreEqual(user.OrdersCount, initialCount);
                Assert.AreEqual(user.Orders.Count(), initialCount);
            }
            using (var db = new OtherContext())
            {
                var invoicesCount = db.Invoices.Count(i => i.UserId == userId);
                Assert.AreEqual(0, invoicesCount);

            }
        }

        [TestMethod]
        public void rollback_transaction_in_multiple_different_contexts_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            MeasurePerf(() =>
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        IncreateUserOrdersCount(db);
                    }

                    using (var db = new OtherContext())
                    {
                        AddInvoice(db);
                    }
                }
            });
        }

        private void MeasurePerf(Action action, [CallerMemberName] string callerName = null)
        {
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < perfLoops; i++)
            {
                action();
            }

            Trace.WriteLine($"[{callerName}] elapsed: {sw.Elapsed}");

            File.AppendAllText(perfLogFile, $"{callerName};{perfLoops};{sw.Elapsed};{sw.ElapsedMilliseconds}\r\n");
        }

        private void AddOrder(MyContext db)
        {
            db.Orders.Add(new Model.Order()
            {
                User = db.Users.Find(userId)
            }); ;

            db.SaveChanges();
        }

        private void IncreateUserOrdersCount(MyContext db)
        {
            var user = db.Users.Find(userId);
            user.OrdersCount++;

            db.SaveChanges();
        }

        private void AddInvoice(OtherContext db)
        {
            db.Invoices.Add(new Invoice()
            {
                UserId = userId
            });
            db.SaveChanges();
        }

        private int GetUserOrdersCount(int userId)
        {
            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                return user.OrdersCount;
            }

        }

        private void PrepareUser(int userId)
        {
            using (var db = new MyContext())
            {
                if (!db.Users.Any(u => u.UserName == "testuser"))
                {
                    db.Users.Add(new Model.User()
                    {
                        Id = userId,
                        UserName = $"testuser"
                    });
                    db.SaveChanges();
                }
            }
        }
    }
}
