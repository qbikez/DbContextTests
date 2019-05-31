using System;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OtherContext;

namespace DbContextTests
{
    [TestClass]
    public class UnitTest1
    {
        private int perfLoops = 1000;

        [TestMethod]
        public void rollback_transaction_in_multiple_same_contexts()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);
            
            using (var tran = new TransactionScope())
            {
                using (var db = new MyContext())
                {
                    var user = db.Users.Find(userId);
                    user.OrdersCount++;

                    db.SaveChanges();
                }

                using (var db = new MyContext())
                {
                    db.Orders.Add(new Model.Order()
                    {
                        User = db.Users.Find(userId)
                    }); ;

                    db.SaveChanges();
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

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < perfLoops; i++)
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        var user = db.Users.Find(userId);
                        user.OrdersCount++;

                        db.SaveChanges();
                    }

                    using (var db = new MyContext())
                    {
                        db.Orders.Add(new Model.Order()
                        {
                            User = db.Users.Find(userId)
                        }); ;

                        db.SaveChanges();
                    }
                }
            }

            Trace.WriteLine($"elapsed: {sw.Elapsed}");

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);

                Assert.AreEqual(initialCount, user.OrdersCount);
                Assert.AreEqual(initialCount, user.Orders.Count());
            }
        }

        [TestMethod]
        public void rollback_transaction_in_single_context_perf()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < perfLoops; i++)
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        var user = db.Users.Find(userId);
                        user.OrdersCount++;

                        db.SaveChanges();

                        db.Orders.Add(new Model.Order()
                        {
                            User = db.Users.Find(userId)
                        }); ;

                        db.SaveChanges();
                    }
                }
            }

            Trace.WriteLine($"elapsed: {sw.Elapsed}");

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);

                Assert.AreEqual(initialCount, user.OrdersCount);
                Assert.AreEqual(initialCount, user.Orders.Count());
            }
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
                    var user = db.Users.Find(userId);
                    user.OrdersCount++;

                    db.SaveChanges();
                }

                using (var db = new OtherContext())
                {
                    db.Invoices.Add(new Invoice()
                    {
                        UserId = userId
                    });
                    db.SaveChanges();
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
                var invoicesCount = db.Invoices.Count(i => i.UserId ==userId);
                Assert.AreEqual(0, invoicesCount);
                
            }
        }

        [TestMethod]
        public void rollback_transaction_in_multiple_different_contexts_peref()
        {
            var userId = 1;
            PrepareUser(userId);

            var initialCount = GetUserOrdersCount(userId);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < perfLoops; i++)
            {
                using (var tran = new TransactionScope())
                {
                    using (var db = new MyContext())
                    {
                        var user = db.Users.Find(userId);
                        user.OrdersCount++;

                        db.SaveChanges();
                    }

                    using (var db = new OtherContext())
                    {
                        db.Invoices.Add(new Invoice()
                        {
                            UserId = userId
                        });
                        db.SaveChanges();
                    }
                }
            }

            Trace.WriteLine($"elapsed: {sw.Elapsed}");

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
