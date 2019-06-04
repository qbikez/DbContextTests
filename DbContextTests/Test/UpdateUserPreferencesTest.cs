using DbContextTests.Services;
using NUnit.Framework;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using DbContextTests.Infrastructure;

namespace DbContextTests.Test
{
    [TestFixture]
    public class UpdateUserPreferencesTest
    {
        private int userId = 4;

        [Test]
        public void update_related_entity_with_context_factory()
        {
            UserTestData.PrepareUser(userId);

            string itemName = $"item-{Guid.NewGuid()}";

            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.ConfigureDirectContext();

                var orderingService = kernel.Get<IOrderingService>();

                // ACT
                orderingService.SetUserPreferences(userId, itemName);
            }

            using (var db = new MyContext())
            {
                var user = db.Users.Include(u => u.UserPreferences).FirstOrDefault(u => u.Id == userId);

                // ups, we updated the object, but didn't reattach it to the new context!
                AssertThat.AreEqual(itemName, user.UserPreferences.FavoriteProduct, AssertOutcome.Inconclusive);
            }
        }

        [Test]
        public void update_related_entity_with_direct_context()
        {
            UserTestData.PrepareUser(userId);

            string itemName = $"item-{Guid.NewGuid()}";
            
            using (var kernel = new Ninject.StandardKernel())
            {
                kernel.ConfigureDirectContext();

                var orderingService = kernel.Get<IOrderingService>();

                // ACT
                orderingService.SetUserPreferences(userId, itemName);
            }

            using (var db = new MyContext())
            {
                var user = db.Users.Include(u => u.UserPreferences).FirstOrDefault(u => u.Id == userId);

                // ups, we updated the object, but didn't reattach it to the new context!
                AssertThat.AreEqual(itemName, user.UserPreferences.FavoriteProduct);
            }
        }

    }
}
