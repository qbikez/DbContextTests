using DbContextTests.Infrastructure;
using DbContextTests.Repositories;
using DbContextTests.Repositories.Impl;
using DbContextTests.Services;
using Ninject;
using System.Data.Entity;

namespace DbContextTests.Test
{
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

        public static void ConfigureDirectContext(this IKernel kernel)
        {
            kernel.BindServices()
                    .UseContextDirectly()
                    .UseSystemTransactions();
        }

        public static void ConfigureContextFactory(this IKernel kernel)
        {
            kernel.BindServices()
                    .UseContextFromFactory()
                    .UseSystemTransactions();
        }
    }
}
