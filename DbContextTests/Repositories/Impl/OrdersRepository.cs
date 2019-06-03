using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbContextTests.Model;

namespace DbContextTests.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly MyContext db;

        public OrdersRepository(MyContext db)
        {
            this.db = db;
        }

        public void Add(Order order)
        {
            db.Orders.Add(order);
            db.SaveChanges();
        }
    }
}
