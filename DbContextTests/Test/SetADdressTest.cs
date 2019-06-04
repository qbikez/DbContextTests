using DbContextTests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using DbContextTests.Infrastructure;
using MyContext.Model;

namespace DbContextTests.Test
{
    [TestClass]
    public class SetAddressTest
    {
        private int userId = 4;

        [TestMethod]
        public void update_value_object_with_context_factory()
        {
            UserTestData.PrepareUser(userId);

            string houseNo = Guid.NewGuid().ToString("n");

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.ConfigureContextFactory();

                var orderingService = kernel.Get<IOrderingService>();

                // ACT
                orderingService.SetUserAddress(userId, new Address()
                {
                    City = "Poznań",
                    HouseNo = houseNo,
                    Street = "Brzęczyszczykiewicza"
                });
            }

            using (var db = new MyContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                Assert.That.AreEqual(houseNo, user.Address.HouseNo);
            }
        }

        [TestMethod]
        public void update_value_object_with_direct_context()
        {
            UserTestData.PrepareUser(userId);

            string houseNo = Guid.NewGuid().ToString("n");

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.ConfigureDirectContext();

                var orderingService = kernel.Get<IOrderingService>();

                // ACT
                orderingService.SetUserAddress(userId, new Address()
                {
                    City = "Poznań",
                    HouseNo = houseNo,
                    Street = "Brzęczyszczykiewicza"
                });
            }

            using (var db = new MyContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                // ups, we updated the object, but didn't reattach it to the new context!
                Assert.That.AreEqual(houseNo, user.Address.HouseNo);
            }
        }
    }
}
