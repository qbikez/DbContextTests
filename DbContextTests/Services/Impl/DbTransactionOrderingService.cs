using DbContextTests.Model;
using DbContextTests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    class DbTransactionOrderingService : IOrderingService
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly IUsersRepository usersRepository;
        private readonly MyContext db;

        public DbTransactionOrderingService(IOrdersRepository ordersRepository, IUsersRepository usersRepository, MyContext db)
        {
            this.ordersRepository = ordersRepository;
            this.usersRepository = usersRepository;
            this.db = db;
        }

        public void MakeOrder(string itemName, int userId)
        {
            // we're using explicit db transaction 
            using (var tran = db.Database.BeginTransaction())
            {
                var order = new Order(itemName, userId);
                ordersRepository.Add(order);
                var user = usersRepository.Find(userId);
                user.IncreaseOrdersCount();
                usersRepository.Update(user);

                tran.Commit();
            }
        }
    }
}
