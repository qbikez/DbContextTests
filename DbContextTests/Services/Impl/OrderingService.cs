﻿using DbContextTests.Model;
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

                if (ThrowAfterOrderAdd) throw new Exception("simulated error after adding order");

                var user = usersRepository.Find(userId);
                user.IncreaseOrdersCount();
                usersRepository.Update(user);

                if (ThrowAfterUserUpdate) throw new Exception("simulated error after updating user counter");

                tran.Commit();
            }
        }

        public bool ThrowAfterOrderAdd { get; set; }
        public bool ThrowAfterUserUpdate { get; set; }
    }
}