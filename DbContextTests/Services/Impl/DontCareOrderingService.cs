using DbContextTests.Model;
using DbContextTests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    class DontCareOrderingService : IOrderingService
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly IUsersRepository usersRepository;

        public DontCareOrderingService(IOrdersRepository ordersRepository, IUsersRepository usersRepository)
        {
            this.ordersRepository = ordersRepository;
            this.usersRepository = usersRepository;
        }

        public void MakeOrder(string itemName, int userId)
        {
            var order = new Order(itemName, userId);
            ordersRepository.Add(order);
            var user = usersRepository.Find(userId);
            user.IncreaseOrdersCount();
            usersRepository.Update(user);
        }
    }
}
