using DbContextTests.Infrastructure;
using DbContextTests.Repositories;
using DbContextTests.Repositories.Impl;
using DbContextTests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public void using_context_commit()
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
                Assert.AreEqual(user.UserPreferences.FavoriteProduct, itemName);

            }
        }

        [TestMethod]
        public void create_order_no_transaction_fail()
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
                Assert.AreEqual(initialUserCount, user.OrdersCount);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);
                Assert.AreEqual(initialCount, ordersCount);
            }
        }

        [TestMethod]
        public void create_order_db_transaction_rollback()
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
        public void using_dbcontextfactory_commit()
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
                orderingService.ShouldUpdatePreference = false;

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
                Assert.AreNotEqual(user.UserPreferences.FavoriteProduct, itemName);
            }
        }

        [TestMethod]
        public void using_dbcontextfactory_update_preference_commit()
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
                Assert.AreEqual(user.UserPreferences.FavoriteProduct, itemName);
            }
        }

        [TestMethod]
        public void using_dbcontextfactory_rollback()
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

    static class KernelExtensions
    {
        public static IKernel BindServices(this IKernel kernel)
        {
            kernel.Bind<IOrderingService, OrderingService>().To<OrderingService>().InScope(ctx => ctx.Kernel);

            return kernel;
        }

        public static IKernel UseContextDirectly(this IKernel kernel)
        {
            kernel.Bind<MyContext, DbContext>().To<MyContext>().InScope(ctx => ctx.Kernel);

            kernel.Bind<IUsersRepository>().To<UsersRepository>().InScope(ctx => ctx.Kernel);
            kernel.Bind<IOrdersRepository>().To<OrdersRepository>().InScope(ctx => ctx.Kernel);

            return kernel;
        }

        public static IKernel UseContextFromFactory(this IKernel kernel)
        {
            kernel.Bind<IUsersRepository>().To<UsersRepositoryWithFactory>().InScope(ctx => ctx.Kernel);
            kernel.Bind<IOrdersRepository>().To<OrdersRepositoryWithFactory>().InScope(ctx => ctx.Kernel);
            kernel.Bind<IContextFactory<MyContext>>().To<MyContextFactory>().InScope(ctx => ctx.Kernel);

            return kernel;
        }

        public static IKernel UseDbTransactions(this IKernel kernel)
        {
            kernel.Bind<ITransactionFactory>().To<DbTransactionFactory>().InScope(ctx => ctx.Kernel);
            return kernel;
        }

        public static IKernel UseSystemTransactions(this IKernel kernel)
        {
            kernel.Bind<ITransactionFactory>().To<SystemTransactionFactory>().InScope(ctx => ctx.Kernel);
            return kernel;
        }

        public static IKernel UseNoTransactions(this IKernel kernel)
        {
            kernel.Bind<ITransactionFactory>().To<NoTransactionFactory>().InScope(ctx => ctx.Kernel);
            return kernel;
        }
    }
}
