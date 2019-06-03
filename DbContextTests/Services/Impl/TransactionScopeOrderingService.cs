using DbContextTests.Model;
using DbContextTests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    class TransactionScopeOrderingService : IOrderingService
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly IUsersRepository usersRepository;

        public TransactionScopeOrderingService(IOrdersRepository ordersRepository, IUsersRepository usersRepository)
        {
            this.ordersRepository = ordersRepository;
            this.usersRepository = usersRepository;
        }

        public void MakeOrder(string itemName, int userId)
        {
            // we're using System.Transactions.TransactionScope to guarantee that
            // users and orders are updated in a single transaction
            // we assume that repositories implement it correctly (as they should)
            using (var tran = new System.Transactions.TransactionScope())
            {
                var order = new Order(itemName, userId);
                ordersRepository.Add(order);

                var user = usersRepository.Find(userId);
                user.IncreaseOrdersCount();
                usersRepository.Update(user);

                tran.Complete();
            }
        }
    }
}
