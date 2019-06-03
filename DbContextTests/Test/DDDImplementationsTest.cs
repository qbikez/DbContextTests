using DbContextTests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Test
{
    [TestClass]
    public class DDDImplementationsTest
    {
        private int userId = 3;

        [TestMethod]
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

        [TestMethod]
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
                Assert.AreNotEqual(user.UserPreferences.FavoriteProduct, itemName);
            }
        }

        [TestMethod]
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
                
                AssertInconclusive(initialCount, ordersCount);                
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void update_related_entity_with_context_factory_fails()
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
                orderingService.ShouldUpdatePreference = true;

                // ACT
                MakeOrder(kernel.Get<IOrderingService>(), itemName);
            }

            // each call to dbcontextfactory.create will create a new instance
            // Assert.AreEqual(3, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);

                Assert.AreEqual(initialUserCount + 1, user.OrdersCount);
                Assert.AreEqual(initialCount + 1, ordersCount);

                // ups, we updated the object, but didn't reattach it to the new context!
                AssertInconclusive(user.UserPreferences.FavoriteProduct, itemName);
            }
        }

        [TestMethod]
        public void update_related_entity_with_direct_context_works()
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
                    .UseSystemTransactions();

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ShouldUpdatePreference = true;

                // ACT
                MakeOrder(kernel.Get<IOrderingService>(), itemName);
            }

            // each call to dbcontextfactory.create will create a new instance
            // Assert.AreEqual(3, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);

                Assert.AreEqual(initialUserCount + 1, user.OrdersCount);
                Assert.AreEqual(initialCount + 1, ordersCount);

                // ups, we updated the object, but didn't reattach it to the new context!
                Assert.AreEqual(user.UserPreferences.FavoriteProduct, itemName);
            }
        }


        private void MakeOrder(IOrderingService orderingService, string itemName = "testitem")
        {
            orderingService.MakeOrder(itemName, userId);
        }

        private void AssertInconclusive<T>(T expected, T actual)
        {
            try
            {
                Assert.AreEqual(expected, actual);                
            }
            catch (AssertFailedException ex)
            {
                Assert.Inconclusive(ex.Message, ex);
            }
            
            Assert.AreNotEqual(expected, actual);
        }
    }
}
