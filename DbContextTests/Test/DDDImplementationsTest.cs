using DbContextTests.Infrastructure;
using DbContextTests.Repositories;
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
    public class DDDImplementations
    {
        private int userId = 3;

        [TestMethod]
        public void create_order_no_errors()
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
                kernel.Bind<IOrderingService>().To<OrderingService>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IUsersRepository>().To<UsersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IOrdersRepository>().To<OrdersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<ITransactionFactory>().To<NoTransactionFactory>().InScope(ctx => ctx.Kernel);
                kernel.Bind<MyContext>().To<MyContext>().InScope(ctx => ctx.Kernel);

                MakeOrder(kernel.Get<IOrderingService>());
            }

            Assert.AreEqual(1, MyContext.TotalInstancesCreated);
            Assert.AreEqual(0, MyContext.InstanceCount);

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                Assert.AreEqual(initialUserCount + 1, user.OrdersCount);
                var ordersCount = db.Orders.Count(o => o.UserId == userId);
                Assert.AreEqual(initialCount + 1, ordersCount);
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
                kernel.Bind<IOrderingService, OrderingService>().To<OrderingService>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IUsersRepository>().To<UsersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IOrdersRepository>().To<OrdersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<ITransactionFactory>().To<NoTransactionFactory>().InScope(ctx => ctx.Kernel);
                kernel.Bind<MyContext>().To<MyContext>().InScope(ctx => ctx.Kernel);

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ThrowAfterOrderAdd = true;

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
                kernel.Bind<IOrderingService, OrderingService>().To<OrderingService>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IUsersRepository>().To<UsersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<IOrdersRepository>().To<OrdersRepository>().InScope(ctx => ctx.Kernel);
                kernel.Bind<ITransactionFactory>().To<DbTransactionFactory>().InScope(ctx => ctx.Kernel);
                kernel.Bind<MyContext, DbContext>().To<MyContext>().InScope(ctx => ctx.Kernel);

                var orderingService = kernel.Get<OrderingService>();
                orderingService.ThrowAfterOrderAdd = true;

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

        private void MakeOrder(IOrderingService orderingService)
        {
            orderingService.MakeOrder("testitem", userId);
        }
    }
}
