using DbContextTests.Services;
using NUnit.Framework;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using DbContextTests.Infrastructure;

namespace DbContextTests.Test
{
    [TestFixture]
    public class MakeOrderTest
    {
        private int userId = 3;

        [Test]
        public void make_order_with_direct_context()
        {
            UserTestData.PrepareUser(userId);

            string itemName = $"item-{Guid.NewGuid()}";
            int initialCount;
            int initialUserCount;

            using (var db = new MyContext())
            {
                initialCount = db.Orders.Count(o => o.UserId == userId);
                initialUserCount = db.Users.Find(userId).OrdersCount;
            }

            MyContext.ResetCounters();

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.BindServices()
                    .UseContextDirectly()
                    .UseNoTransactions();                

                MakeOrder(kernel.Get<IOrderingService>(), itemName);
            }

            Assert.AreEqual(1, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);

                Assert.AreEqual(initialUserCount + 1, user.OrdersCount);
                Assert.AreEqual(initialCount + 1, ordersCount);
            }
        }

        [Test]
        public void make_order_with_context_factory()
        {
            UserTestData.PrepareUser(userId);

            string itemName = $"item-{Guid.NewGuid()}";
            int initialCount;
            int initialUserCount;

            using (var db = new MyContext())
            {
                initialCount = db.Orders.Count(o => o.UserId == userId);
                initialUserCount = db.Users.Find(userId).OrdersCount;
            }

            MyContext.ResetCounters();

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.BindServices()
                    .UseContextFromFactory()
                    .UseSystemTransactions();

                var orderingService = kernel.Get<OrderingService>();

                // ACT
                MakeOrder(kernel.Get<IOrderingService>(), itemName);
            }

            // each call to dbcontextfactory.create will create a new instance
            Assert.AreEqual(3, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);

                Assert.AreEqual(initialUserCount + 1, user.OrdersCount);
                Assert.AreEqual(initialCount + 1, ordersCount);
            }
        }

        [Test]
        public void make_order_without_transaction_leaves_inconsistent_db()
        {
            UserTestData.PrepareUser(userId);

            int initialCount;
            int initialUserCount;

            using (var db = new MyContext())
            {
                initialCount = db.Orders.Count(o => o.UserId == userId);
                initialUserCount = db.Users.Find(userId).OrdersCount;
            }

            MyContext.ResetCounters();

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.BindServices()
                    .UseContextDirectly()
                    .UseNoTransactions();

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ShouldThrowAfterOrderAdd = true;

                try
                {
                    // ACT
                    MakeOrder(kernel.Get<IOrderingService>());
                    Assert.Fail("expected exception");
                } catch
                {
                    // ignore
                }
            }

            Assert.AreEqual(1, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);
                Assert.AreEqual(initialUserCount, user.OrdersCount);

                // will be incosistent:                
                AssertThat.AreEqual(initialCount, ordersCount, AssertOutcome.Inconclusive);                
            }
        }

        [Test]
        public void make_order_with_transaction_leaves_consistent_db()
        {
            UserTestData.PrepareUser(userId);

            int initialCount;
            int initialUserCount;

            using (var db = new MyContext())
            {
                initialCount = db.Orders.Count(o => o.UserId == userId);
                initialUserCount = db.Users.Find(userId).OrdersCount;
            }

            MyContext.ResetCounters();

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.BindServices()
                    .UseContextDirectly()
                    .UseDbTransactions();

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ShouldThrowAfterOrderAdd = true;

                try
                {
                    // ACT
                    MakeOrder(kernel.Get<IOrderingService>());
                    Assert.Fail("expected exception");
                }
                catch
                {
                    // ignore
                }
            }

            Assert.AreEqual(1, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                Assert.AreEqual(initialUserCount, user.OrdersCount);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);
                Assert.AreEqual(initialCount, ordersCount);
            }
        }

        [Test]
        public void make_order_with_context_factory_leaves_consistent_db()
        {
            UserTestData.PrepareUser(userId);

            int initialCount;
            int initialUserCount;

            using (var db = new MyContext())
            {
                initialCount = db.Orders.Count(o => o.UserId == userId);
                initialUserCount = db.Users.Find(userId).OrdersCount;
            }

            MyContext.ResetCounters();

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.BindServices()
                    .UseContextFromFactory()
                    .UseSystemTransactions();

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ShouldThrowAfterOrderAdd = true;

                try
                {
                    // ACT
                    MakeOrder(kernel.Get<IOrderingService>());
                    Assert.Fail("expected exception");
                }
                catch
                {
                    // ignore
                }
            }

            Assert.AreEqual(1, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);

                Assert.AreEqual(initialUserCount, user.OrdersCount);
                Assert.AreEqual(initialCount, ordersCount);
            }
        }

        private void MakeOrder(IOrderingService orderingService, string itemName = "testitem")
        {
            orderingService.MakeOrder(itemName, userId);
        }      

    }
}
