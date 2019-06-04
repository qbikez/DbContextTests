using DbContextTests.Model;
using DbContextTests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    class OrderingService : IOrderingService
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly IUsersRepository usersRepository;
        private readonly ITransactionFactory transactionFactory;

        public OrderingService(IOrdersRepository ordersRepository, IUsersRepository usersRepository, ITransactionFactory transactionFactory)
        {
            this.ordersRepository = ordersRepository;
            this.usersRepository = usersRepository;
            this.transactionFactory = transactionFactory;
        }

        public void MakeOrder(string itemName, int userId)
        {
            using (var tran = transactionFactory.GetTransaction())
            {
                var order = new Order(itemName, userId);
                ordersRepository.Add(order);

                if (ShouldThrowAfterOrderAdd) throw new Exception("simulated error after adding order");

                var user = usersRepository.Find(userId);

                user.IncreaseOrdersCount();

                usersRepository.Update(user);

                if (ShouldThrowAfterUserUpdate) throw new Exception("simulated error after updating user counter");

                tran.Commit();
            }
        }

        public void SetUserPreferences(int userId, string favoriteItem)
        {
            var user = usersRepository.Find(userId);

            user.UpdatePreference(p => p.FavoriteProduct = favoriteItem);

            usersRepository.Update(user);
        }

        public bool ShouldThrowAfterOrderAdd { get; set; }
        public bool ShouldThrowAfterUserUpdate { get; set; }
    }
}
