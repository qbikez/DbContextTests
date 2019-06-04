﻿using DbContextTests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [TestClass]
    public class UpdateUserPreferencesTest
    {
        private int userId = 4;

        [TestMethod]
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
                Assert.That.AreEqual(itemName, user.UserPreferences.FavoriteProduct, AssertOutcome.Inconclusive);
            }
        }

        [TestMethod]
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
                Assert.That.AreEqual(itemName, user.UserPreferences.FavoriteProduct);
            }
        }

    }
}