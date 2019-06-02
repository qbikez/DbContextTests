using DbContextTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    [TestClass]
    public class UsingContextTest
    {
        private int userId = 2;
        private PerformanceMeter perfMeter;
        public UsingContextTest()
        {
            perfMeter = new PerformanceMeter("using-perf.csv", 1000);
        }

        [TestMethod]
        public void get_and_update_single_context()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() => {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                using (var db = new MyContext())
                {
                    var user = db.Users.Find(userId);

                    if (user.UserName != newUsername)
                    {
                        user.UserName = newUsername;
                    }

                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_reattach()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() => {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                }

                if (user.UserName != newUsername)
                {
                    user.UserName = newUsername;
                }

                using (var db = new MyContext())
                {
                    db.Users.Attach(user);
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    user.UserName = newUsername;
                    
                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_retrieve()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() => {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                }

                if (user.UserName != newUsername)
                {
                    user.UserName = newUsername;
                }

                using (var db = new MyContext())
                {
                    var dbUser = db.Users.Find(userId);
                    
                    dbUser.UserName = user.UserName;

                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_bad()
        {
            UserTestData.PrepareUser(userId);

            var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
            User user;

            using (var db = new MyContext())
            {
                user = db.Users.Find(userId);
            }

            if (user.UserName != newUsername)
            {
                user.UserName = newUsername;
            }

            using (var db = new MyContext())
            {
                user.UserName = newUsername;

                db.SaveChanges();
            }

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);

                // the assingment won't do anything, since the user object is from another context
                Assert.AreNotEqual(newUsername, user1.UserName);
            }
        }
    }
}
