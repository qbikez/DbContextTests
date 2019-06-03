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
        private int userId = 1;
        private PerformanceMeter perfMeter;

        public TransactionScopeTest()
        {
            perfMeter = new PerformanceMeter("perf.csv", 1000);
        }

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
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            perfMeter.MeasurePerf(() =>
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
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            perfMeter.MeasurePerf(() =>
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

        private void AddOrder(MyContext db)
        {
            db.Orders.Add(new Model.Order("item-1")
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

        private void PrepareUser(int userId) => UserTestData.PrepareUser(userId);
    }
}
