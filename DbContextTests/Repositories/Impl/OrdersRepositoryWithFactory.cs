using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbContextTests.Infrastructure;
using DbContextTests.Model;

namespace DbContextTests.Repositories.Impl
{
    class OrdersRepositoryWithFactory : IOrdersRepository
    {
        private readonly IContextFactory<MyContext> contextFactory;

        public OrdersRepositoryWithFactory(IContextFactory<MyContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        public void Add(Order order)
        {
            using(var db = contextFactory.Create())
            {
                db.Orders.Add(order);
            }
        }
    }
}
